using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateSiteUrl;

public class UpdateSiteUrlMaintenanceProvider(
    ISiteService siteService,
    ShellSettings shellSettings,
    IOptions<UpdateSiteUrlMaintenanceOptions> options) : MaintenanceProviderBase
{
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(options.Value.IsEnabled && !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var siteSettings = await siteService.LoadSiteSettingsAsync();
        siteSettings.BaseUrl = TenantUrlHelpers.GetEvaluatedValueForTenant(
            options.Value.DefaultTenantSiteUrl,
            options.Value.SiteUrl,
            shellSettings,
            options.Value.SiteUrlFromTenantName);

        await siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
