using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.Extensions.DependencyInjection;
using static Lombiq.Hosting.Tenants.Management.Constants.FeatureNames;

namespace Lombiq.Hosting.Tenants.Management.Extensions;

public static class LombiqHostingTenantsManagementExtensions
{
    /// <summary>
    /// Registers Lombiq.Hosting.Tenants.Management.HideRecipesFromSetup as a setup feature and hides recipes from the
    /// setup screen based on configurable tags.
    /// </summary>
    /// <param name="tags">
    /// Recipe tags which will be hidden. If you leave this parameter empty then you can use the default,
    /// "HideFromSetupScreen" tag.
    /// </param>
    public static OrchardCoreBuilder HideRecipesByTagsFromSetup(this OrchardCoreBuilder builder, params string[] tags) =>
        builder
            .AddSetupFeatures(HideRecipesFromSetup)
            .ConfigureServices(services =>
                services.Configure<HideRecipesFromSetupOptions>(options =>
                {
                    if (tags.Length != 0)
                    {
                        options.HiddenTags = tags;
                    }
                }));
}
