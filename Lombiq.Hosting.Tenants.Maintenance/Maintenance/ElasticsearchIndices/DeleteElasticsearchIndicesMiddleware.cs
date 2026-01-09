using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class DeleteElasticsearchIndicesMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ShellSettings _shellSettings;

    private readonly IShellSettingsManager _shellSettingsManager;

    private readonly IDistributedLock _distributedLock;

    public DeleteElasticsearchIndicesMiddleware(
        RequestDelegate next,
        ShellSettings shellSettings,
        IShellSettingsManager shellSettingsManager,
        IDistributedLock distributedLock)
    {
        _next = next;
        _shellSettings = shellSettings;
        _shellSettingsManager = shellSettingsManager;
        _distributedLock = distributedLock;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Method != WebRequestMethods.Http.Get)
        {
            await _next.Invoke(httpContext);
            return;
        }

        if (await InvokeNextIfUninitializedAsync(_shellSettings, httpContext)) return;

        // Try to acquire a lock before starting installation
        var (locker, locked) = await _distributedLock.TryAcquireLockAsync(
            "ELASTICSEARCH_INDICES_DELETION",
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));

        if (!locked)
        {
            throw new TimeoutException($"Failed to acquire an Elasticsearch indices deletion lock for the tenant: {_shellSettings.Name}");
        }

        await using var acquiredLock = locker;

        // Check if the tenant was installed by another instance.
        if (await InvokeNextIfUninitializedAsync(_shellSettings, httpContext)) return;

        using var settings = (await _shellSettingsManager.LoadSettingsAsync(_shellSettings.Name)).AsDisposable();

        // If the tenant was initialized by another instance, then skip again.
        if (await InvokeNextIfUninitializedAsync(settings, httpContext)) return;

        var elasticIndexManager = httpContext.RequestServices.GetRequiredService<ElasticsearchIndexManager>();

        // Delete all tenant specific indexes in Elasticsearch.
        await elasticIndexManager.DeleteAllIndexesAsync();

        await _next.Invoke(httpContext);
    }

    private async Task<bool> InvokeNextIfUninitializedAsync(ShellSettings shellSettings, HttpContext httpContext)
    {
        if (shellSettings.IsUninitialized())
        {
            return false;
        }

        await _next.Invoke(httpContext);
        return true;
    }
}
