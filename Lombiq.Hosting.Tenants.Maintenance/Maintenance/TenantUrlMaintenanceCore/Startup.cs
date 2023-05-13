using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.TenantUrlMaintenanceCore;

[Feature(FeatureNames.TenantUrlMaintenanceCore)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new TenantUrlMaintenanceOptions();
        var configSection = _shellConfiguration.GetSection("Lombiq_Hosting_Tenants_Maintenance:TenantUrlMaintenance");
        configSection.Bind(options);
        services.Configure<TenantUrlMaintenanceOptions>(configSection);
    }
}
