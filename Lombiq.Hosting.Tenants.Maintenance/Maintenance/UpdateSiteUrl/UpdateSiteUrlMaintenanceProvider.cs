using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateSiteUrl;

public class UpdateSiteUrlMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ISiteService _siteService;
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<UpdateSiteUrlMaintenanceOptions> _options;

    public UpdateSiteUrlMaintenanceProvider(
        ISiteService siteService,
        ShellSettings shellSettings,
        IOptions<UpdateSiteUrlMaintenanceOptions> options)
    {
        _siteService = siteService;
        _shellSettings = shellSettings;
        _options = options;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.Value.IsEnabled && !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        siteSettings.BaseUrl = TenantUrlHelpers.GetEvaluatedValueForTenant(
            _options.Value.DefaultTenantSiteUrl,
            _options.Value.SiteUrl,
            _shellSettings);

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
