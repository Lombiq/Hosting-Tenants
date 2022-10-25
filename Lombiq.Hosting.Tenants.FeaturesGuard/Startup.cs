using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.FeaturesGuard;

[Feature(FeatureNames.FeaturesGuard)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public Startup(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ConditionallyEnabledFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_Tenants_FeaturesGuard:ConditionallyEnabledFeaturesOptions:ConditionallyEnabledFeatures")
                .Bind(options));

        services.AddScoped<IFeatureEventHandler, FeaturesEventHandler>();

        // probably not usable at all
        if (_shellSettings.IsDefaultShell()) // reversed
        {
            var whatsThisThen = _shellConfiguration.GetSection(
                "Lombiq_Hosting_Tenants_FeaturesGuard:ConditionallyEnabledFeaturesOptions:ConditionallyEnabledFeatures");
            var value = whatsThisThen.Get<ConditionallyEnabledFeaturesOptions>();

            var iable = "";
            //foreach (var alwaysEnabledFeature in alwaysEnabledFeatures.AlwaysEnabledFeatures)
            //{
            //    services.AddTransient(service => new ShellFeature(alwaysEnabledFeature, alwaysEnabled: true));
            //}
        }

        // these necessary even?
        services.AddTransient(service => new ShellFeature(FeatureNames.Users, alwaysEnabled: true));
        services.AddTransient(service => new ShellFeature(FeatureNames.Roles, alwaysEnabled: true));
    }
}
