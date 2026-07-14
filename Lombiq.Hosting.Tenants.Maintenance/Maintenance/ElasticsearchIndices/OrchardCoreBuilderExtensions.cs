using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds Elasticsearch setup and tenant features, if <paramref name="useElasticsearch"/> is <see langword="true"/>.
    /// </summary>
    public static OrchardCoreBuilder AddElasticsearchFeatures(this OrchardCoreBuilder builder, IWebHostEnvironment environment, bool useElasticsearch)
    {
        if (!useElasticsearch) return builder;

        builder.AddSetupFeatures(FeatureNames.DeleteElasticsearchIndicesBeforeSetup);

        if (environment.IsStaging())
        {
            builder.AddTenantFeatures(FeatureNames.DeleteOrRebuildElasticsearchIndices);
        }

        return builder;
    }
}
