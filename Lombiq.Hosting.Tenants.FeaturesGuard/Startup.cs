using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.FeaturesGuard;

[Feature(FeatureNames.FeaturesGuard)]
public class Startup(IShellConfiguration shellConfiguration) : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IFeatureEventHandler, FeaturesEventHandler>();

        services.Configure<ConditionallyEnabledFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_Tenants_FeaturesGuard:ConditionallyEnabledFeaturesOptions:ConditionallyEnabledFeatures")
                .Bind(options));
    }
}
