using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core;
using System.IO;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteMediaCache;

public class DeleteMediaCacheMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<DeleteMediaCacheMaintenanceOptions> _options;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ShellSettings _shellSettings;

    public DeleteMediaCacheMaintenanceProvider(
        IOptions<DeleteMediaCacheMaintenanceOptions> options,
        IWebHostEnvironment webHostEnvironment,
        ShellSettings shellSettings)
    {
        _options = options;
        _webHostEnvironment = webHostEnvironment;
        _shellSettings = shellSettings;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            _shellSettings.Name == ShellSettings.DefaultShellName &&
            // Should run on every swap, although not every new deployment. No better simple way to check for this, so
            // running on every deployment anyway.
            context.LatestExecution?.BuildVersion != context.CurrentExecution.BuildVersion);

    public override Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var cachePath = Path.Combine(
            _webHostEnvironment.WebRootPath,
            DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, recursive: true);
        }

        return Task.CompletedTask;
    }
}
