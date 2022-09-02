using Lombiq.Hosting.MultiTenancy.Tenants.Services;
using OrchardCore.Environment.Extensions.Features;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds the Features Guard feature's <see cref="TenantFeatureBuilderEvents" /> service to the service collection.
    /// </summary>
    public static void AddFeaturesGuard(this OrchardCoreBuilder builder) =>
        builder.ApplicationServices.AddSingleton<IFeatureBuilderEvents, TenantFeatureBuilderEvents>();
}
