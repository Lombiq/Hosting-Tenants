using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateSiteUrl;

[Feature(FeatureNames.UpdateSiteUrl)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.BindAndConfigureSection<UpdateSiteUrlMaintenanceOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:UpdateSiteUrl");

        services.AddScoped<IMaintenanceProvider, UpdateSiteUrlMaintenanceProvider>();
    }
}
