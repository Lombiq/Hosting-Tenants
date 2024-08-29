using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndexes;

public class DeleteElasticsearchIndexesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<DeleteElasticsearchIndexesMaintenanceOptions> _options;
    private readonly ElasticIndexManager _elasticIndexManager;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public DeleteElasticsearchIndexesMaintenanceProvider(
        IOptions<DeleteElasticsearchIndexesMaintenanceOptions> options,
        ElasticIndexManager elasticIndexManager,
        ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _options = options;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var indexes = (await _elasticIndexSettingsService.GetSettingsAsync())
            .Select(index => index.IndexName)
            .ToList();

        await _elasticIndexManager.DeleteIndex("*");
    }
}
