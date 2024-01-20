using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateSiteUrl;

[Feature(FeatureNames.UpdateSiteUrl)]
public class Startup(IShellConfiguration shellConfiguration) : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new UpdateSiteUrlMaintenanceOptions();
        var configSection = shellConfiguration.GetSection("Lombiq_Hosting_Tenants_Maintenance:UpdateSiteUrl");
        configSection.Bind(options);
        services.Configure<UpdateSiteUrlMaintenanceOptions>(configSection);

        services.AddScoped<IMaintenanceProvider, UpdateSiteUrlMaintenanceProvider>();
    }
}
