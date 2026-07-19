using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.PurgeMediaCache;

public class PurgeMediaCacheMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<PurgeMediaCacheMaintenanceOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    public PurgeMediaCacheMaintenanceProvider(
        IOptions<PurgeMediaCacheMaintenanceOptions> options,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    // Should run on every swap, although not every new deployment. No better simple way to check for this, so running
    // on every deployment anyway.
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            context.LatestExecution?.BuildVersion != context.CurrentExecution.BuildVersion);

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context) =>
        _serviceProvider.GetService<IMediaFileStoreCacheFileProvider>()?.PurgeAsync() ?? Task.CompletedTask;
}
