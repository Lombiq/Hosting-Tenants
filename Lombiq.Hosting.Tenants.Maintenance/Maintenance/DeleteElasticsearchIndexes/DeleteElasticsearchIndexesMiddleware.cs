using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndexes;

public class DeleteElasticsearchIndexesMiddleware
{
    private readonly RequestDelegate _next;

    private readonly IShellHost _shellHost;

    private readonly ShellSettings _shellSettings;

    private readonly IShellSettingsManager _shellSettingsManager;

    private readonly IDistributedLock _distributedLock;

    public DeleteElasticsearchIndexesMiddleware(
        RequestDelegate next,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IShellSettingsManager shellSettingsManager,
        IDistributedLock distributedLock)
    {
        _next = next;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _shellSettingsManager = shellSettingsManager;
        _distributedLock = distributedLock;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!_shellSettings.IsUninitialized())
        {
            await _next.Invoke(httpContext);
            return;
        }

        // Try to acquire a lock before starting installation
        var (locker, locked) = await _distributedLock.TryAcquireLockAsync(
            "ELASTICSERACH_INDICES_DELETION_LOCK",
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));

        if (!locked)
        {
            throw new TimeoutException($"Fails to acquire an elasticsearch indices deletion lock for the tenant: {_shellSettings.Name}");
        }

        await using var acquiredLock = locker;

        // Check if the tenant was installed by another instance.
        if (!_shellSettings.IsUninitialized())
        {
            await _next.Invoke(httpContext);
            return;
        }

        var pathBase = httpContext.Request.PathBase;
        if (!pathBase.HasValue)
        {
            pathBase = "/";
        }

        using var settings = (await _shellSettingsManager
                .LoadSettingsAsync(_shellSettings.Name))
            .AsDisposable();

        // If the tenant was initialized by another instance, reload the shell context and redirect to the path base.
        if (!settings.IsUninitialized())
        {
            await _shellHost.ReloadShellContextAsync(_shellSettings, eventSource: false);
            httpContext.Response.Redirect(pathBase);

            return;
        }

        var elasticIndexManager = httpContext.RequestServices.GetRequiredService<ElasticIndexManager>();
        await elasticIndexManager.DeleteIndex("*");

        await _next.Invoke(httpContext);
    }
}
