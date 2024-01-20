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

public class MaintenanceManager(
    IOrchardClock clock,
    ILogger<MaintenanceManager> logger,
    IEnumerable<IMaintenanceProvider> maintenanceProviders,
    ISession session,
    IShellHost shellHost,
    ShellSettings shellSettings) : IMaintenanceManager
{
    public Task<MaintenanceTaskExecutionData> GetLatestExecutionByMaintenanceIdAsync(string maintenanceId) =>
        session.Query<MaintenanceTaskExecutionData, MaintenanceTaskExecutionIndex>(collection: DocumentCollections.Maintenance)
            .Where(execution => execution.MaintenanceId == maintenanceId)
            .OrderByDescending(execution => execution.ExecutionTimeUtc)
            .FirstOrDefaultAsync();

    public async Task ExecuteMaintenanceTasksAsync()
    {
        var orderedProviders = maintenanceProviders.OrderBy(provider => provider.Order);
        foreach (var provider in orderedProviders)
        {
            var currentExecution = new MaintenanceTaskExecutionData
            {
                MaintenanceId = provider.Id,
                ExecutionTimeUtc = clock.UtcNow,
            };
            var context = new MaintenanceTaskExecutionContext
            {
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
        logger.LogDebug("Executing maintenance task {MaintenanceId}, if needed.", provider.Id);

        if (await provider.ShouldExecuteAsync(context))
        {
            try
            {
                await provider.ExecuteAsync(context);
                execution.IsSuccess = string.IsNullOrEmpty(execution.Error);
                if (execution.IsSuccess)
                {
                    logger.LogDebug("Maintenance task {MaintenanceId} executed successfully.", provider.Id);
                }
                else
                {
                    logger.LogError(
                        "Maintenance task {MaintenanceId} executed with error: {Error}",
                        provider.Id,
                        execution.Error);
                }
            }
            catch (Exception exception) when (!exception.IsFatal())
            {
                execution.IsSuccess = false;
                execution.Error = exception.ToString();

                logger.LogError(
                    exception,
                    "Maintenance task {MaintenanceId} failed to execute due to an exception.",
                    provider.Id);
            }

            await session.SaveAsync(execution, collection: DocumentCollections.Maintenance);
            await session.SaveChangesAsync();

            if (context.ReloadShellAfterMaintenanceCompletion) await shellHost.ReloadShellContextAsync(shellSettings);
        }
        else
        {
            logger.LogDebug("Maintenance task {MaintenanceId} is not needed.", provider.Id);
        }
    }
}
