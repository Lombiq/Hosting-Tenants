using Lombiq.Hosting.BuildVersionDisplay.Models;
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

public class RebuildElasticsearchIndexesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ElasticsearchIndexManager _elasticsearchIndexManager;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;

    public RebuildElasticsearchIndexesMaintenanceProvider(
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

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.RebuildMaintenanceIsEnabled &&
            (!context.WasLatestExecutionSuccessful() || ShouldExecute(context.LatestExecution)));

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        RebuildAsync(_indexProfileManager, _indexProfileStore, _elasticsearchIndexManager);

    private static bool ShouldExecute(MaintenanceTaskExecutionData contextLatestExecution)
    {
        var previousOrchardVersion = new Version(contextLatestExecution?.OrchardVersion ?? "0.0.0.0");
        var currentOrchardVersion = new Version(new BuildVersionModel().OrchardVersion);

        // It is always necessary to rebuild the indexes when upgrading to OC 3.0.0 or newer.
        return previousOrchardVersion.Major <= 2 && currentOrchardVersion.Major >= 3;
    }

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
                // This is the same thing you see in the ~/Admin/indexing/rebuild/{id} action, just batched for all
                // Elasticsearch indexes.
                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
                await elasticsearchIndexManager.RebuildAsync(indexProfile);
                await indexProfileManager.SynchronizeAsync(indexProfile);
            });
    }
}
