using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

[Feature(FeatureNames.UpdateShellRequestUrls)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new UpdateShellRequestUrlMaintenanceOptions();
        var configSection = _shellConfiguration.GetSection("Lombiq_Hosting_Tenants_Maintenance:UpdateShellRequestUrl");
        configSection.Bind(options);
        services.Configure<UpdateShellRequestUrlMaintenanceOptions>(configSection);

        services.AddScoped<IMaintenanceProvider, UpdateShellRequestUrlsMaintenanceProvider>();
    }
}
