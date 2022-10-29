using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Makes <see cref="ConditionallyEnabledFeaturesOptions"/> available for use.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuard(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var shellConfiguration = serviceProvider
                .GetRequiredService<IShellConfiguration>()
                .GetSection(
                    "Lombiq_Hosting_Tenants_FeaturesGuard:ConditionallyEnabledFeaturesOptions:ConditionallyEnabledFeatures");

            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options => shellConfiguration.Bind(options));
        });

        return builder;
    }

    /// <summary>
    /// Makes <see cref="ConditionallyEnabledFeaturesOptions"/> available for use with test values in UI tests.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuardForUITesting(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // unbind first? or why does it not overwrite

            var shellConfiguration = serviceProvider
                .GetRequiredService<IShellConfiguration>()
                .GetSection(
                    "Lombiq_Hosting_Tenants_FeaturesGuard:TestConditionallyEnabledFeaturesOptions:ConditionallyEnabledFeatures");

            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options => shellConfiguration.Bind(options));
        });

        return builder;
    }
}
