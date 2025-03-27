using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class DeleteElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;
    private readonly IElasticsearchIndexManager _elasticIndexManager;

    public DeleteElasticsearchIndicesMaintenanceProvider(
        IOptions<ElasticsearchIndicesMaintenanceOptions> options,
        IElasticsearchIndexManager elasticIndexManager)
    {
        _options = options;
        _elasticIndexManager = elasticIndexManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.DeleteMaintenanceIsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        // Delete all tenant specific indexes in Elasticsearch.
        _elasticIndexManager.DeleteIndex("*");
}
