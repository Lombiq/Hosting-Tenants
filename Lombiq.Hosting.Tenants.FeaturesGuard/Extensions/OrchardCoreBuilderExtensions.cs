using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Binds Azure Storage as the conditional feature and Media as the condition feature to
    /// <see cref="ConditionallyEnabledFeaturesOptions"/>.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuardForAzureStorage(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, _) =>
            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options =>
                options.EnableFeatureIfOtherFeatureIsEnabled = new Dictionary<string, IEnumerable<string>>
                {
                    ["OrchardCore.Media.Azure.Storage"] = new List<string> { "OrchardCore.Media" },
                }));

        return builder;
    }

    /// <summary>
    /// Binds the provided dictionary's keys and values as the conditional and condition features to
    /// <see cref="ConditionallyEnabledFeaturesOptions"/>.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuard(
        this OrchardCoreBuilder builder, IDictionary<string, IEnumerable<string>> configDictionary)
    {
        builder.ConfigureServices((tenantServices, _) =>
            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options =>
                options.EnableFeatureIfOtherFeatureIsEnabled = configDictionary));

        return builder;
    }
}
