using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndices;

public class RebuildElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;
    private readonly ElasticIndexingService _elasticIndexingService;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public RebuildElasticsearchIndicesMaintenanceProvider(
        IOptions<ElasticsearchIndicesMaintenanceOptions> options,
        ElasticIndexingService elasticIndexingService,
        ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _options = options;
        _elasticIndexingService = elasticIndexingService;
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.RebuildMaintenanceIsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var settings = await _elasticIndexSettingsService.GetSettingsAsync();
        foreach (var setting in settings)
        {
            await _elasticIndexingService.RebuildIndexAsync(setting);

            if (setting.QueryAnalyzerName != setting.AnalyzerName)
            {
                // Query Analyzer may be different until the index in rebuilt.
                // Since the index is rebuilt, lets make sure we query using the same analyzer.
                setting.QueryAnalyzerName = setting.AnalyzerName;

                await _elasticIndexSettingsService.UpdateIndexAsync(setting);
            }

            await _elasticIndexingService.ProcessContentItemsAsync(setting.IndexName);
        }
    }
}
