using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class RebuildElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ElasticsearchIndexManager _elasticsearchIndexManager;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;

    public RebuildElasticsearchIndicesMaintenanceProvider(
        ElasticsearchIndexManager elasticsearchIndexManager,
        IIndexProfileStore indexProfileStore,
        IOptions<ElasticsearchIndicesMaintenanceOptions> options)
    {
        _elasticsearchIndexManager = elasticsearchIndexManager;
        _indexProfileStore = indexProfileStore;
        _options = options;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.Value.RebuildMaintenanceIsEnabled && context.IsFailedOrOutdated());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        MigrateAsync(_elasticsearchIndexManager, _indexProfileStore);

    public static async Task MigrateAsync(
        ElasticsearchIndexManager elasticsearchIndexManager,
        IIndexProfileStore indexProfileStore)
    {
        var indexProfiles = await indexProfileStore.GetAllElasticsearchIndexesAsync();

        await indexProfiles
            .AwaitEachAsync(async indexProfile =>
            {
                await elasticsearchIndexManager.RebuildAsync(indexProfile);

                var analyzerName = indexProfile.GetOrCreate<ElasticsearchIndexMetadata>().AnalyzerName;
                var queryAnalyzerName = indexProfile.GetOrCreate<ElasticsearchDefaultQueryMetadata>().QueryAnalyzerName;
                if (queryAnalyzerName != analyzerName)
                {
                    // Query Analyzer may be different until the index is rebuilt.
                    // Since the index is rebuilt, lets make sure we query using the same analyzer.
                    indexProfile.Alter<ElasticsearchDefaultQueryMetadata>(
                        setting => setting.QueryAnalyzerName = analyzerName);
                }

                // Without this, the connection may remain open, causing a concurrent access exception when we query
                // anything from the database using the same underlying session.
                await indexProfileStore.UpdateAsync(indexProfile);
            });
    }
}
