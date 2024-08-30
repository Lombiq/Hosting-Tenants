using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndices;

public class DeleteElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<DeleteElasticsearchIndicesMaintenanceOptions> _options;
    private readonly ElasticIndexManager _elasticIndexManager;

    public DeleteElasticsearchIndicesMaintenanceProvider(
        IOptions<DeleteElasticsearchIndicesMaintenanceOptions> options,
        ElasticIndexManager elasticIndexManager)
    {
        _options = options;
        _elasticIndexManager = elasticIndexManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.MaintenanceIsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        // Delete all tenant specific indexes in Elasticsearch.
        _elasticIndexManager.DeleteIndex("*");
}
