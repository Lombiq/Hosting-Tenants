using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using static Lombiq.Hosting.Tenants.Management.Constants.FeatureNames;
namespace Lombiq.Hosting.Tenants.Management.Extensions
{
    public static class LombiqHostingTenantsManagementExtensions
    {
        public static OrchardCoreBuilder HideRecipesByTagsFromSetup(this OrchardCoreBuilder builder, params string[] tags) =>
            builder
                .AddSetupFeatures(HideRecipesFromSetup)
                .ConfigureServices(services =>
                    services.Configure<HideRecipesFromSetupOptions>(settings =>
                        settings.HiddenTags = settings.HiddenTags.Concat(tags)));
    }
}
