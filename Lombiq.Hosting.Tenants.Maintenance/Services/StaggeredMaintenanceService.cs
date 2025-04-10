using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class StaggeredMaintenanceService : IStaggeredMaintenanceService
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IShellHost _shellHost;
    private readonly IContentManager _contentManager;
    private readonly ILogger<StaggeredMaintenanceService> _logger;
    private readonly ISession _session;
    private readonly IClock _clock;

    public StaggeredMaintenanceService(
        IShellHost shellHost,
        IContentManager contentManager,
        ILogger<StaggeredMaintenanceService> logger,
        ISession session,
        IClock clock)
    {
        _shellHost = shellHost;
        _contentManager = contentManager;
        _logger = logger;
        _session = session;
        _clock = clock;
    }

    public async Task<StaggeredMaintenancePart> RunScheduledMaintenanceForAllTenantAsync(bool newVersion = false, bool reset = false)
    {
        // Only one thread can run the maintenance at a time, we don't want to run it in parallel. Also, we don't want
        // to run after each other, so we use a SemaphoreSlim to limit the number of concurrent threads to 1 and check
        // if the current count is 0 before running the maintenance.
        if (_lock.CurrentCount == 0)
        {
            return null;
        }

        await _lock.WaitAsync();

        var staggeredMaintenancePart = (await GetorCreateStaggeredMaintenanceAsync()).As<StaggeredMaintenancePart>();
        try
        {
            await StaggeredMaintenanceAsync(staggeredMaintenancePart, newVersion, reset);
        }
        finally
        {
            staggeredMaintenancePart.Finish(_clock);
            await SaveSettingsAsync(staggeredMaintenancePart);
            _lock.Release();
        }

        return staggeredMaintenancePart;
    }

    public async Task<ContentItem> GetorCreateStaggeredMaintenanceAsync()
    {
        var staggeredContentItem =
            await _session.Query<ContentItem, ContentItemIndex>(item => item.ContentType == ContentTypes.StaggeredMaintenance)
                .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(staggeredContentItem?.ContentItemId))
        {
            staggeredContentItem = await _contentManager.NewAsync(ContentTypes.StaggeredMaintenance);
            await _contentManager.CreateAsync(staggeredContentItem);
        }

        return staggeredContentItem;
    }

    private async Task StaggeredMaintenanceAsync(
        StaggeredMaintenancePart staggeredMaintenancePart,
        bool newVersion,
        bool reset)
    {
        staggeredMaintenancePart.Start(_clock, nameof(RunScheduledMaintenanceForAllTenantAsync), newVersion, reset);

        var remainingTenants = GetRemainingTenants(staggeredMaintenancePart);

        while (remainingTenants.Count != 0)
        {
            if (staggeredMaintenancePart.ShouldCancel(nameof(RunScheduledMaintenanceForAllTenantAsync))) return;

            await RunStaggeredMaintenanceForEachTenantAsync(
                remainingTenants,
                staggeredMaintenancePart);

            // Calculate percentage of completed tenants.
            staggeredMaintenancePart.CalculatePercentage();

            await SaveSettingsAsync(staggeredMaintenancePart);
            await _session.SaveChangesAsync();

            // Get the remaining tenants after processing, so if new tenant is added it could be proccessed in the next run
            remainingTenants = GetRemainingTenants(staggeredMaintenancePart);

            if (remainingTenants.Count != 0 && await WaitBeforeNextAsync(staggeredMaintenancePart))
            {
                return;
            }
        }
    }

    /// <summary>
    /// Waits for the specified time before processing the next tenant. This is to avoid overwhelming the system with
    /// requests and also checking periodically if the maintenance was cancelled.
    /// </summary>
    /// <returns><see langword="true"/> if the maintenance was cancelled, <see langword="false"/> otherwise.</returns>
    private async Task<bool> WaitBeforeNextAsync(StaggeredMaintenancePart staggeredMaintenancePart)
    {
        var waited = TimeSpan.Zero;
        var delay = staggeredMaintenancePart.TimeSpanBetweenBatches.Value!.Value;
        var delayCheckInterval = TimeSpan.FromMilliseconds(500);
        while (waited < delay)
        {
            if (staggeredMaintenancePart.ShouldCancel(nameof(RunScheduledMaintenanceForAllTenantAsync)))
            {
                _logger.LogInformation("Maintenance cancelled during delay wait. Exiting.");
                return true;
            }

            var waitTime = delay - waited < delayCheckInterval ? delay - waited : delayCheckInterval;
            await Task.Delay(waitTime);
            waited += waitTime;
        }

        return false;
    }

    private List<ShellSettings> GetRemainingTenants(StaggeredMaintenancePart staggeredMaintenancePart)
    {
        var allTenants = GetAllRunningTenantSettingsExceptDefault().ToList();

        var take = (int)staggeredMaintenancePart.ProcessingStep.Value!;
        staggeredMaintenancePart.AllTenantCount.Value = allTenants.Count;

        return allTenants.Where(settings => !staggeredMaintenancePart.ProcessedTenantIds.Contains(settings.TenantId))
            .Take(take)
            .ToList();
    }

    private IEnumerable<ShellSettings> GetAllRunningTenantSettingsExceptDefault() =>
        _shellHost.GetAllSettings()
            .Where(settings => settings.Name != ShellSettings.DefaultShellName && settings.IsRunning());

    private async Task RunStaggeredMaintenanceForEachTenantAsync(
        List<ShellSettings> remainingTenants,
        StaggeredMaintenancePart staggeredMaintenancePart)
    {
        foreach (var remainingTenant in remainingTenants)
        {
            try
            {
                await _shellHost.WithShellScopeAsync(
                    scope =>
                    {
                        // Only logging is necessary here, as the actual maintenance and migration tasks are already done
                        // when we get here.
                        var tenantLogger = scope.ServiceProvider.GetRequiredService<ILogger<StaggeredMaintenanceService>>();
                        tenantLogger.LogError(
                            "Staggered maintenance for current tenant finished successfully for maintenance version {Version}.",
                            staggeredMaintenancePart.CurrentVersion.Value);
                        return Task.CompletedTask;
                    },
                    remainingTenant.Name);

                await SaveMaintenanceVersionAsync(
                    staggeredMaintenancePart,
                    staggeredMaintenancePart.CurrentVersion.Value.ToTechnicalString(),
                    remainingTenant.Name);

                _logger.LogError(
                    "Staggered maintenance for tenant '{TenantName}' finished successfully for maintenance version {Version}.",
                    remainingTenant.Name,
                    staggeredMaintenancePart.CurrentVersion.Value);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Staggered maintenance for tenant '{TenantName}' for maintenance version {Version} failed.",
                    remainingTenant.Name,
                    staggeredMaintenancePart.CurrentVersion.Value);
                staggeredMaintenancePart.AddErrorLog(remainingTenant.Name, exception.Message);
            }
            finally
            {
                // We should always add the tenant to the processed list, even if it failed.
                staggeredMaintenancePart.ProcessTenant(remainingTenant.TenantId);
            }
        }
    }

    private Task SaveMaintenanceVersionAsync(
        StaggeredMaintenancePart staggeredMaintenancePart,
        string version,
        string tenantName)
    {
        staggeredMaintenancePart.AddVersion(tenantName, version);
        return SaveSettingsAsync(staggeredMaintenancePart);
    }

    private Task SaveSettingsAsync(ContentPart part)
    {
        part.Apply();
        return _contentManager.CreateOrUpdateAsync(part.ContentItem);
    }
}
