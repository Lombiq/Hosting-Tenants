using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Lombiq.Hosting.Tenants.HealthChecks.Constants;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Enables the "Lombiq Hosting - Tenants Health Checks" module's features.
    /// </summary>
    public static OrchardCoreBuilder EnableTenantHealthChecks(this OrchardCoreBuilder orchardCoreBuilder) =>
        orchardCoreBuilder.EnableTenantHealthChecks(configuration: null);

    /// <summary>
    /// Enables the "Lombiq Hosting - Tenants Health Checks" module's features, if the <paramref name="configuration"/>
    /// value in <see cref="ConfigurationKeys.IsEnabled"/> is <see langword="true"/>.
    /// </summary>
    public static OrchardCoreBuilder EnableTenantHealthChecks(
        this OrchardCoreBuilder orchardCoreBuilder,
        IConfiguration configuration) =>
        configuration == null || configuration.GetValue(ConfigurationKeys.IsEnabled, defaultValue: false)
            ? orchardCoreBuilder
                .AddTenantFeatures(HealthChecksFeatureIds.AllTenants)
                .AddDefaultTenantFeatures(HealthChecksFeatureIds.DefaultTenant)
            : orchardCoreBuilder;
}
