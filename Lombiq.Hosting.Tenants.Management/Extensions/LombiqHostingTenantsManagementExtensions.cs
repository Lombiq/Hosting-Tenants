using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.Extensions.DependencyInjection;
using static Lombiq.Hosting.Tenants.Management.Constants.FeatureNames;
namespace Lombiq.Hosting.Tenants.Management.Extensions
{
    public static class LombiqHostingTenantsManagementExtensions
    {
        public static OrchardCoreBuilder HideRecipesByCategoryFromSetup(this OrchardCoreBuilder builder, params string[] categories) =>
            builder
                .AddSetupFeatures(HideRecipesFromSetup)
                .ConfigureServices(services =>
                    services.Configure<HideRecipesFromSetupOptions>(settings =>
                        settings.HiddenCategories = categories));
    }
}
