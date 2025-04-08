using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class StaggeredMaintenanceService : IStaggeredMaintenanceService
{
    private readonly IShellHost _shellHost;
    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly ILogger<StaggeredMaintenanceService> _logger;

    public StaggeredMaintenanceService(
        IShellHost shellHost,
        IContentManager contentManager,
        ISiteService siteService,
        ILogger<StaggeredMaintenanceService> logger)
    {
        _shellHost = shellHost;
        _contentManager = contentManager;
        _siteService = siteService;
        _logger = logger;
    }

    public async Task<StaggeredMaintenancePart> RunScheduledMaintenanceForAllTenantAsync(bool newVersion = false, bool reset = false)
    {
        MaintenanceJobStore.Clear(nameof(RunScheduledMaintenanceForAllTenantAsync));
        // Get or create the StaggeredMaintenance.
        var staggeredContentItem = await GetStaggeredMaintenanceAsync();
        var staggeredMaintenancePart = staggeredContentItem.As<StaggeredMaintenancePart>();

        if (newVersion || staggeredMaintenancePart.CurrentVersion.Value == 0)
        {
            staggeredMaintenancePart.CurrentVersion.Value++;
        }

        if (newVersion || reset)
        {
            staggeredMaintenancePart.ProcessedTenantIds.Clear();
            staggeredMaintenancePart.ErrorLogs.Clear();
            staggeredMaintenancePart.AllTenantCount.Value = 0;
            staggeredMaintenancePart.ProcessedTenantsCount.Value = 0;
            staggeredMaintenancePart.ProgressPercentage.Value = 0;
        }

        staggeredMaintenancePart.AllTenantCount.Value = GetAllRunningTenantSettingsExceptDefault().Count();
        var remainingTenants = GetRemainingTenants(staggeredMaintenancePart);

        while (remainingTenants.Count != 0)
        {
            if (MaintenanceJobStore.IsCancelled(nameof(RunScheduledMaintenanceForAllTenantAsync))) return staggeredMaintenancePart;

            await RunStaggeredMaintenanceForEachTenantAsync(remainingTenants, staggeredMaintenancePart);

            // Calculate percentage of completed tenants.
            staggeredMaintenancePart.ProgressPercentage.Value =
                staggeredMaintenancePart.ProcessedTenantsCount.Value /
                staggeredMaintenancePart.AllTenantCount.Value * 100;

            await SaveSettingsAsync(staggeredContentItem, staggeredMaintenancePart, _siteService);

            // Get the remaining tenants after processing, so if new tenant is added it could be proccessed in the next run
            remainingTenants = GetRemainingTenants(staggeredMaintenancePart);
        }

        return staggeredMaintenancePart;
    }

    private List<ShellSettings> GetRemainingTenants(StaggeredMaintenancePart staggeredMaintenancePart)
    {
        var allTenants = GetAllRunningTenantSettingsExceptDefault().ToList();

        var take = staggeredMaintenancePart.ProcessingStep.Value != null
            ? (int)staggeredMaintenancePart.ProcessingStep.Value
            : 1;
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
                    async scope =>
                    {
                        // Only logging is necessary here, as the actual maintenance and migration tasks are already done
                        // when we get here.
                        var tenantLogger = scope.ServiceProvider.GetRequiredService<ILogger<StaggeredMaintenanceService>>();
                        var tenantSiteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
                        var tenantContentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

                        await SaveMaintenanceStatusAsync(
                            staggeredMaintenancePart,
                            tenantSiteService,
                            tenantContentManager);

                        tenantLogger.LogError(
                            "Staggered maintenance for tenant '{TenantName}' finished successfully for maintenance version {Version}.",
                            remainingTenant.Name,
                            staggeredMaintenancePart.CurrentVersion.Value);
                    },
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
                staggeredMaintenancePart.ErrorLogs.Add(remainingTenant.Name, exception.Message);
            }
            finally
            {
                // Save the changes to the database in StaggeredMaintenance and StaggeredMaintenanceStatus.
                staggeredMaintenancePart.ProcessedTenantIds.Add(remainingTenant.TenantId);
                staggeredMaintenancePart.ProcessedTenantsCount.Value++;
            }
        }
    }

    private async Task<ContentItem> GetStaggeredMaintenanceAsync()
    {
        var staggeredContentItem =
            await _siteService.GetSettingsAsync<ContentItem>(ContentTypes.StaggeredMaintenance);

        return string.IsNullOrEmpty(staggeredContentItem.ContentItemId)
            ? await _contentManager.NewAsync(ContentTypes.StaggeredMaintenance)
            : staggeredContentItem;
    }

    private static async Task<ContentItem> GetStaggeredMaintenanceStatusAsync(
        ISiteService tenantSiteService,
        IContentManager tenantContentManager)
    {
        var staggeredMaintenanceTenantStatusContentItem =
            await tenantSiteService.GetSettingsAsync<ContentItem>(ContentTypes.StaggeredMaintenanceStatus);
        return string.IsNullOrEmpty(staggeredMaintenanceTenantStatusContentItem.ContentItemId)
            ? await tenantContentManager.NewAsync(ContentTypes.StaggeredMaintenanceStatus)
            : staggeredMaintenanceTenantStatusContentItem;
    }

    private static async Task SaveMaintenanceStatusAsync(
        StaggeredMaintenancePart staggeredMaintenancePart,
        ISiteService tenantSiteService,
        IContentManager tenantContentManager)
    {
        var maintenanceStatus = await GetStaggeredMaintenanceStatusAsync(tenantSiteService, tenantContentManager);
        var maintenanceStatusPart = maintenanceStatus.As<StaggeredMaintenanceTenantStatusPart>();

        maintenanceStatusPart.Version.Value = staggeredMaintenancePart.CurrentVersion.Value;

        await SaveSettingsAsync(maintenanceStatus, maintenanceStatusPart, tenantSiteService);
    }

    private static async Task SaveSettingsAsync(
        ContentItem contentItem,
        ContentPart part,
        ISiteService siteService)
    {
        part.Apply();
        var siteSettings = await siteService.LoadSiteSettingsAsync();
        siteSettings.Properties[contentItem.ContentType] = JObject.FromObject(contentItem);
        await siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
