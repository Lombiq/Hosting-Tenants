using Lombiq.Hosting.MultiTenancy.Tenants.Handlers;
using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Lombiq.Hosting.MultiTenancy.Tenants;

[Feature("FeaturesGuard")]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AlwaysEnabledFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_MultiTenancy_Tenants:AlwaysEnabledFeaturesOptions")
                .Bind(options));

        services.AddScoped<IFeatureEventHandler, FeaturesEventHandler>();

        services.AddTransient(service => new ShellFeature("OrchardCore.Users", alwaysEnabled: true));
    }
}
