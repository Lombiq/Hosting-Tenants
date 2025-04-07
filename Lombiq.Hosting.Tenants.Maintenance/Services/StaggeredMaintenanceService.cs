using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Settings;
using System;
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

    public async Task RunScheduledMaintenanceForAllTenantAsync()
    {
        // Get or create the StaggeredMaintenance.
        var staggeredContentItem = await GetStaggeredMaintenanceAsync();
        var staggeredMaintenancePart = staggeredContentItem.As<StaggeredMaintenancePart>();

        var tenantNames = _shellHost.GetAllSettings()
            .Where(settings => settings.Name != ShellSettings.DefaultShellName && settings.IsRunning())
            .Select(settings => settings.Name)
            .ToList();

        staggeredMaintenancePart.AllTenantCount.Value = tenantNames.Count;
        staggeredMaintenancePart.CurrentVersion.Value++;

        foreach (var tenantName in tenantNames)
        {
            try
            {
                await _shellHost.WithShellScopeAsync(
                    async scope =>
                    {
                        // Only logging is necessary here, as the actual maintenance and migration tasks are already done
                        // when we get here.
                        var tenantLogger =
                            scope.ServiceProvider.GetRequiredService<ILogger<StaggeredMaintenanceService>>();
                        var tenantSiteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
                        var tenantContentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

                        await UpdateMaintenanceStatusAsync(
                            staggeredMaintenancePart,
                            tenantSiteService,
                            tenantContentManager);

                        tenantLogger.LogError(
                            "Staggered maintenance for tenant '{TenantName}' finished successfully for maintenance version {Version}.",
                            tenantName,
                            staggeredMaintenancePart.CurrentVersion.Value);
                    },
                    tenantName);

                _logger.LogError(
                    "Staggered maintenance for tenant '{TenantName}' finished successfully for maintenance version {Version}.",
                    tenantName,
                    staggeredMaintenancePart.CurrentVersion.Value);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Staggered maintenance for tenant '{TenantName}' for maintenance version {Version} failed.",
                    tenantName,
                    staggeredMaintenancePart.CurrentVersion.Value);
                staggeredMaintenancePart.ErrorLogs.Add(tenantName, exception.Message);
            }
            finally
            {
                // Save the changes to the database in StaggeredMaintenance and StaggeredMaintenanceStatus.
                staggeredMaintenancePart.ProcessedTenantNames.Add(tenantName);
                staggeredMaintenancePart.ProcessedTenantsCount.Value++;

                // Calculate percentage of completed tenants.
                staggeredMaintenancePart.ProgressPercentage.Value =
                    staggeredMaintenancePart.ProcessedTenantsCount.Value /
                    staggeredMaintenancePart.AllTenantCount.Value * 100;
                staggeredContentItem.Apply(staggeredMaintenancePart);
            }
        }

        await SaveSettingsAsync(staggeredContentItem, staggeredMaintenancePart, _siteService);
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
            await tenantSiteService.GetSettingsAsync<ContentItem>(ContentTypes
                .StaggeredMaintenanceStatus);
        return string.IsNullOrEmpty(staggeredMaintenanceTenantStatusContentItem.ContentItemId)
            ? await tenantContentManager.NewAsync(ContentTypes.StaggeredMaintenanceStatus)
            : staggeredMaintenanceTenantStatusContentItem;
    }

    private static async Task UpdateMaintenanceStatusAsync(
        StaggeredMaintenancePart staggeredMaintenancePart,
        ISiteService tenantSiteService,
        IContentManager tenantContentManager)
    {
        var maintenanceStatus =
            await GetStaggeredMaintenanceStatusAsync(tenantSiteService, tenantContentManager);

        var maintenanceStatusPart = maintenanceStatus.As<StaggeredMaintenanceTenantStatusPart>();
        maintenanceStatusPart.Version.Value = staggeredMaintenancePart.CurrentVersion.Value;

        await SaveSettingsAsync(maintenanceStatus, maintenanceStatusPart, tenantSiteService);
    }

    private static async Task SaveSettingsAsync(
        ContentItem contentItem,
        ContentPart part,
        ISiteService siteService)
    {
        part.Apply(contentItem);
        var siteSettings = await siteService.LoadSiteSettingsAsync();
        siteSettings.Properties[contentItem.ContentType] = JObject.FromObject(contentItem);
        await siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
