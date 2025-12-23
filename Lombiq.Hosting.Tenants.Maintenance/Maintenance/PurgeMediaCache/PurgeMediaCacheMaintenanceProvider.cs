using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.PurgeMediaCache;

public class PurgeMediaCacheMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<PurgeMediaCacheMaintenanceOptions> _options;
    private readonly IMediaFileStoreCacheFileProvider _mediaFileStoreCacheFileProvider;

    public PurgeMediaCacheMaintenanceProvider(
        IOptions<PurgeMediaCacheMaintenanceOptions> options,
        IMediaFileStoreCacheFileProvider mediaFileStoreCacheFileProvider)
    {
        _options = options;
        _mediaFileStoreCacheFileProvider = mediaFileStoreCacheFileProvider;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            // Should run on every swap, although not every new deployment. No better simple way to check for this, so
            // running on every deployment anyway.
            context.LatestExecution?.BuildVersion != context.CurrentExecution.BuildVersion);

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        _mediaFileStoreCacheFileProvider.PurgeAsync();
}
