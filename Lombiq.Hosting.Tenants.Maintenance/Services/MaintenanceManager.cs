#nullable enable

using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using IOrchardClock = OrchardCore.Modules.IClock;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class MaintenanceManager : IMaintenanceManager
{
    private readonly IOrchardClock _clock;
    private readonly ILogger<MaintenanceManager> _logger;
    private readonly IEnumerable<IMaintenanceProvider> _maintenanceProviders;
    private readonly ISession _session;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    public MaintenanceManager(
        IOrchardClock clock,
        ILogger<MaintenanceManager> logger,
        IEnumerable<IMaintenanceProvider> maintenanceProviders,
        ISession session,
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        _clock = clock;
        _logger = logger;
        _maintenanceProviders = maintenanceProviders;
        _session = session;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
    }

    public Task<MaintenanceTaskExecutionData?> GetLatestExecutionByMaintenanceIdAsync(string maintenanceId) =>
        _session
            .Query<MaintenanceTaskExecutionData, MaintenanceTaskExecutionIndex>(collection: DocumentCollections.Maintenance)
            .Where(execution => execution.MaintenanceId == maintenanceId)
            .OrderByDescending(execution => execution.ExecutionTimeUtc)
            .FirstOrDefaultAsync()!;

    public async Task ExecuteMaintenanceTasksAsync()
    {
        var orderedProviders = _maintenanceProviders.OrderBy(provider => provider.Order);
        foreach (var provider in orderedProviders)
        {
            var context = new MaintenanceTaskExecutionContext
            {
                LatestExecution = await GetLatestExecutionByMaintenanceIdAsync(provider.Id),
                CurrentExecution = MaintenanceTaskExecutionData.FromProvider(provider, _clock.UtcNow),
            };

            await ExecuteMaintenanceTaskIfNeededAsync(provider, context);
        }
    }

    public async Task DeleteMaintenanceExecutionsByIdAsync(string maintenanceId)
    {
        var executions = await _session
            .Query<MaintenanceTaskExecutionData, MaintenanceTaskExecutionIndex>(collection: DocumentCollections.Maintenance)
            .Where(execution => execution.MaintenanceId == maintenanceId)
            .ListAsync();

        foreach (var execution in executions)
        {
            _session.Delete(execution, collection: DocumentCollections.Maintenance);
        }
    }

    public IMaintenanceProvider? GetProviderById(string maintenanceId) =>
        _maintenanceProviders.FirstOrDefault(provider => provider.Id == maintenanceId);

    public async Task<MaintenanceTaskExecutionData?> ExecuteMaintenanceTaskIfNeededAsync(
        IMaintenanceProvider provider,
        MaintenanceTaskExecutionContext context,
        bool forceExecute = false)
    {
        _logger.LogDebug("Executing maintenance task {MaintenanceId}, if needed.", provider.Id);

        if (!forceExecute && !await provider.ShouldExecuteAsync(context))
        {
            _logger.LogDebug("Maintenance task {MaintenanceId} is not needed.", provider.Id);
            return null;
        }

        var execution = context.CurrentExecution;

        try
        {
            await provider.ExecuteAsync(context);
            if (execution.IsSuccess)
            {
                _logger.LogDebug("Maintenance task {MaintenanceId} executed successfully.", provider.Id);
                execution.ExecutionEndUtc = _clock.UtcNow;
            }
            else
            {
                var isWarning = execution.IsWarning;
                _logger.Log(
                    isWarning ? LogLevel.Warning : LogLevel.Error,
                    "Maintenance task {MaintenanceId} executed with {Type}: {Error}",
                    provider.Id,
                    isWarning ? "warning" : "error",
                    execution.Error);
            }

            // We must use SaveChangesAsync and not FlushAsync, otherwise the migration will fail after site reset. See
            // https://github.com/Lombiq/Hosting-Tenants/pull/182 for details.
            await _session.SaveAsync(execution, collection: DocumentCollections.Maintenance);
            await _session.SaveChangesAsync();
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            execution.Error = exception.ToString();

            _logger.LogError(
                exception,
                "Maintenance task {MaintenanceId} failed to execute due to an exception.",
                provider.Id);
        }

        if (context.ReloadShellAfterMaintenanceCompletion) await _shellHost.ReloadShellContextAsync(_shellSettings);
        return execution;
    }
}
