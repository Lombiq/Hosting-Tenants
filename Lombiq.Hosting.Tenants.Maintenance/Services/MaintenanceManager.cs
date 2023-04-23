using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class MaintenanceManager : IMaintenanceManager
{
    private readonly IClock _clock;
    private readonly ILogger<MaintenanceManager> _logger;
    private readonly IEnumerable<IMaintenanceProvider> _maintenanceProviders;
    private readonly ISession _session;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    public MaintenanceManager(
        IClock clock,
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

    public Task<MaintenanceTaskExecutionData> GetLatestExecutionByMaintenanceIdAsync(string maintenanceId) =>
        _session.Query<MaintenanceTaskExecutionData, MaintenanceTaskExecutionIndex>(collection: DocumentCollections.Maintenance)
            .Where(execution => execution.MaintenanceId == maintenanceId)
            .OrderByDescending(execution => execution.ExecutionTimeUtc)
            .FirstOrDefaultAsync();

    public async Task ExecuteMaintenanceTasksAsync()
    {
        var orderedProviders = _maintenanceProviders.OrderBy(p => p.Order);
        foreach (var provider in orderedProviders)
        {
            var currentExecution = new MaintenanceTaskExecutionData
            {
                MaintenanceId = provider.Id,
                ExecutionTimeUtc = _clock.UtcNow,
            };
            var context = new MaintenanceTaskExecutionContext
            {
                // GetLatestExecutionTask = () => GetLatestExecutionByMaintenanceIdAsync(provider.Id),
                LatestExecution = await GetLatestExecutionByMaintenanceIdAsync(provider.Id),
                CurrentExecution = currentExecution,
            };

            await ExecuteMaintenanceTaskIfNeededAsync(provider, context, currentExecution);
        }
    }

    private async Task ExecuteMaintenanceTaskIfNeededAsync(
        IMaintenanceProvider provider,
        MaintenanceTaskExecutionContext context,
        MaintenanceTaskExecutionData execution)
    {
        _logger.LogDebug("Executing maintenance task {MaintenanceId}, if needed.", provider.Id);

        if (await provider.ShouldExecuteAsync(context))
        {
            try
            {
                await provider.ExecuteAsync(context);
                execution.IsSuccess = string.IsNullOrEmpty(execution.Error);
                if (execution.IsSuccess)
                {
                    _logger.LogDebug("Maintenance task {MaintenanceId} executed successfully.", provider.Id);
                }
                else
                {
                    _logger.LogError(
                        "Maintenance task {MaintenanceId} executed with error: {Error}",
                        provider.Id,
                        execution.Error);
                }
            }
            catch (Exception exception) when (!exception.IsFatal())
            {
                execution.IsSuccess = false;
                execution.Error = exception.ToString();

                _logger.LogError(
                    exception,
                    "Maintenance task {MaintenanceId} failed to execute due to an exception.",
                    provider.Id);
            }

            _session.Save(execution, collection: DocumentCollections.Maintenance);
            await _session.SaveChangesAsync();

            if (context.ReloadShellAfterMaintenanceCompletion) await _shellHost.ReloadShellContextAsync(_shellSettings);
        }
        else
        {
            _logger.LogDebug("Maintenance task {MaintenanceId} is not needed.", provider.Id);
        }
    }
}
