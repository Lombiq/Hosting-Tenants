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
                options.EnableFeatureIfOtherFeatureIsEnabled = new Dictionary<string, string>
                {
                    ["OrchardCore.Media.Azure.Storage"] = "OrchardCore.Media",
                }));

        return builder;
    }
}
