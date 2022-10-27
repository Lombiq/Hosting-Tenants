using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.FeaturesGuard;

[Feature(FeatureNames.FeaturesGuard)]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IFeatureEventHandler, FeaturesEventHandler>();
}
