using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

[Feature(FeatureNames.UpdateShellRequestUrls)]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IMaintenanceProvider, UpdateShellRequestUrlsMaintenanceProvider>();
}
