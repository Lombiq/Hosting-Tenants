using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class RebuildElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ElasticsearchIndexManager _elasticsearchIndexManager;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;

    public RebuildElasticsearchIndicesMaintenanceProvider(
        ElasticsearchIndexManager elasticsearchIndexManager,
        IIndexProfileManager indexProfileManager,
        IIndexProfileStore indexProfileStore,
        IOptions<ElasticsearchIndicesMaintenanceOptions> options)
    {
        _elasticsearchIndexManager = elasticsearchIndexManager;
        _indexProfileManager = indexProfileManager;
        _indexProfileStore = indexProfileStore;
        _options = options;
    }

    // We use IsFailedOrOutdated, which triggers on every new build, to work around problematic backend changes in OC v3
    // that make it necessary to fully rebuild after deployment.
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.Value.RebuildMaintenanceIsEnabled && context.IsFailedOrOutdated());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        RebuildAsync(_indexProfileManager, _indexProfileStore, _elasticsearchIndexManager);

    [Obsolete($"Use {nameof(RebuildAsync)} instead.")]
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

    public static async Task RebuildAsync(
        IIndexProfileManager indexProfileManager,
        IIndexProfileStore indexProfileStore,
        ElasticsearchIndexManager elasticsearchIndexManager)
    {
        var indexProfiles = await indexProfileStore.GetAllElasticsearchIndexesAsync();

        await indexProfiles
            .AwaitEachAsync(async indexProfile =>
            {
                // This is the same thing you see in the "~/Admin/indexing/rebuild/{id}" action. It's just batched for
                // all Elasticsearch indexes. Note that even after SynchronizeAsync below, the actual reindexing process
                // continues in the Elasticsearch server in the background. When triggering rebuild via the admin UI,
                // this is also communicated with the success message: "An index has been rebuilt successfully. The
                // synchronizing process was triggered in the background."
                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
                await elasticsearchIndexManager.RebuildAsync(indexProfile);
                await indexProfileManager.SynchronizeAsync(indexProfile);
            });
    }
}
