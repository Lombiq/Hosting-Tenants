using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class RebuildElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;
    private readonly IDocumentManager<ElasticIndexSettingsDocument> _documentManager;
    private readonly ElasticIndexingService _elasticIndexingService;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public RebuildElasticsearchIndicesMaintenanceProvider(
        IOptions<ElasticsearchIndicesMaintenanceOptions> options,
        IDocumentManager<ElasticIndexSettingsDocument> documentManager,
        ElasticIndexingService elasticIndexingService,
        ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _options = options;
        _documentManager = documentManager;
        _elasticIndexingService = elasticIndexingService;
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.RebuildMaintenanceIsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        MigrateAsync(_documentManager, _elasticIndexingService, _elasticIndexSettingsService);

    public static async Task MigrateAsync(
        IDocumentManager<ElasticIndexSettingsDocument> documentManager,
        ElasticIndexingService elasticIndexingService,
        ElasticIndexSettingsService elasticIndexSettingsService)
    {
        var settings = await elasticIndexSettingsService.GetSettingsAsync();
        foreach (var setting in settings)
        {
            await elasticIndexingService.RebuildIndexAsync(setting);

            if (setting.QueryAnalyzerName != setting.AnalyzerName)
            {
                // Query Analyzer may be different until the index is rebuilt.
                // Since the index is rebuilt, lets make sure we query using the same analyzer.
                setting.QueryAnalyzerName = setting.AnalyzerName;

                await elasticIndexSettingsService.UpdateIndexAsync(setting);

                // Without this, the connection may remain open, causing a concurrent access exception when we query
                // anything from the database using the same underlying session.
                if (documentManager is DocumentManager<ElasticIndexSettingsDocument> { DocumentStore: { } store })
                {
                    await store.CommitAsync();
                }
            }

            await elasticIndexingService.ProcessContentItemsAsync(setting.IndexName);
        }
    }
}
