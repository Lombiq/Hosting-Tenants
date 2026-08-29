using Lombiq.Hosting.BuildVersionDisplay.Models;
using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Elasticsearch.Core.Services;
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
        ResetAsync(_indexProfileManager, _indexProfileStore, _elasticsearchIndexManager);

    private static bool ShouldExecute(MaintenanceTaskExecutionData contextLatestExecution)
    {
        var previousOrchardVersion = new Version(contextLatestExecution?.OrchardVersion ?? "0.0.0.0");
        var currentOrchardVersion = new Version(new BuildVersionModel().OrchardVersion);

        // It is always necessary to rebuild the indexes when upgrading to OC 3.0.0 or newer.
        return previousOrchardVersion.Major <= 2 && currentOrchardVersion.Major >= 3;
    }

    public static async Task ResetAsync(
        IIndexProfileManager indexProfileManager,
        IIndexProfileStore indexProfileStore,
        ElasticsearchIndexManager elasticsearchIndexManager)
    {
        var indexProfiles = await indexProfileStore.GetAllElasticsearchIndexesAsync();

        await indexProfiles
            .AwaitEachAsync(async indexProfile =>
            {
                // This is the same thing you see in the ~/Admin/indexing/reset/{id} action. It's just batched for all
                // Elasticsearch indexes. Note that even after SynchronizeAsync below, the actual reindexing process
                // continues in the Elasticsearch server in the background. When triggering rebuild via the admin UI,
                // this is also communicated with the success message: "An index has been rebuilt successfully. The
                // synchronizing process was triggered in the background."
                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
                await indexProfileManager.SynchronizeAsync(indexProfile);
            });
    }
}
