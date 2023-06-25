using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Binds Azure Storage as the conditional feature and Media as the condition feature to
    /// <see cref="ConditionallyEnabledFeaturesOptions"/>.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuardForAzureStorage(this OrchardCoreBuilder builder) =>
        builder.ConfigureFeaturesGuardWithoutOverriding(
            new Dictionary<string, IEnumerable<string>>
            {
                ["OrchardCore.Media.Azure.Storage"] = new List<string> { "OrchardCore.Media" },
            });

    /// <summary>
    /// Binds Elasticsearch as the conditional feature and Search or Indexing as the condition feature to
    /// <see cref="ConditionallyEnabledFeaturesOptions"/>.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuardForElasticsearch(this OrchardCoreBuilder builder) =>
        builder.ConfigureFeaturesGuardWithoutOverriding(
            new Dictionary<string, IEnumerable<string>>
            {
                ["OrchardCore.Search.Elasticsearch"] =
                    new List<string> { "OrchardCore.Search", "OrchardCore.Indexing" },
            });

    /// <summary>
    /// Binds the provided dictionary's keys and values as the conditional and condition features to
    /// <see cref="ConditionallyEnabledFeaturesOptions"/>.
    /// </summary>
    public static OrchardCoreBuilder ConfigureFeaturesGuard(
        this OrchardCoreBuilder builder, IDictionary<string, IEnumerable<string>> configDictionary)
    {
        builder.ConfigureServices((tenantServices, _) =>
            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options =>
            {
                options.EnableFeatureIfOtherFeatureIsEnabled.Clear();
                options.EnableFeatureIfOtherFeatureIsEnabled.AddRange(configDictionary);
            }));

        return builder;
    }

    public static OrchardCoreBuilder ConfigureFeaturesGuardWithoutOverriding(
        this OrchardCoreBuilder builder,
        IDictionary<string, IEnumerable<string>> configDictionary)
    {
        builder.ConfigureServices((tenantServices, _) =>
            tenantServices.PostConfigure<ConditionallyEnabledFeaturesOptions>(options =>
            {
                if (!options.EnableFeatureIfOtherFeatureIsEnabled.Any())
                {
                    options.EnableFeatureIfOtherFeatureIsEnabled.AddRange(configDictionary);
                }
                else
                {
                    options.EnableFeatureIfOtherFeatureIsEnabled.AddRange(
                        configDictionary.Where(dictionaryItem =>
                            !options.EnableFeatureIfOtherFeatureIsEnabled.ContainsKey(dictionaryItem.Key)));
                }
            }));

        return builder;
    }
}
