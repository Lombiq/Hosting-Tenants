using Elastic.Clients.Elasticsearch;
using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class DeleteElasticsearchIndicesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ElasticsearchIndicesMaintenanceOptions> _options;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly IShellConfiguration _shellConfiguration;

    public DeleteElasticsearchIndicesMaintenanceProvider(
        IOptions<ElasticsearchIndicesMaintenanceOptions> options,
        ElasticsearchClient elasticsearchClient,
        IShellConfiguration shellConfiguration)
    {
        _options = options;
        _elasticsearchClient = elasticsearchClient;
        _shellConfiguration = shellConfiguration;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.DeleteMaintenanceIsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        _elasticsearchClient.DeleteAllIndexesAsync(_shellConfiguration);
}
