using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateTenantUrl;

public class UpdateSiteUrlMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ISiteService _siteService;
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<UpdateTenantUrlMaintenanceOptions> _options;

    public UpdateSiteUrlMaintenanceProvider(
        ISiteService siteService,
        ShellSettings shellSettings,
        IOptions<UpdateTenantUrlMaintenanceOptions> options)
    {
        _siteService = siteService;
        _shellSettings = shellSettings;
        _options = options;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        !_options.Value.Enabled
            ? Task.FromResult(false)
            : Task.FromResult(!context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        siteSettings.BaseUrl = TenantUrlHelper.GetTenantUrl(_options.Value, _shellSettings);
        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
