using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndices;

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
            throw new TimeoutException($"Fails to acquire an elasticsearch indices deletion lock for the tenant: {_shellSettings.Name}");
        }

        await using var acquiredLock = locker;

        // Check if the tenant was installed by another instance.
        if (await InvokeNextIfUninitializedAsync(_shellSettings, httpContext)) return;

        using var settings = (await _shellSettingsManager.LoadSettingsAsync(_shellSettings.Name)).AsDisposable();

        // If the tenant was initialized by another instance, then skip again.
        if (await InvokeNextIfUninitializedAsync(settings, httpContext)) return;

        var elasticIndexManager = httpContext.RequestServices.GetRequiredService<ElasticIndexManager>();

        // Delete all tenant specific indexes in Elasticsearch.
        await elasticIndexManager.DeleteIndex("*");

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

    private static ConnectionSettings GetConnectionSettings(ElasticConnectionOptions elasticConfiguration)
    {
        // This is a copy of the OC Elasticsearch module's OrchardCore.Search.Elasticsearch.Startup.GetConnectionSettings method.
#pragma warning disable CA2000 // Call System. IDisposable. Dispose on object created by
        // 'GetConnectionPool(elasticConfiguration)' before all references to it are out of scope
        var pool = GetConnectionPool(elasticConfiguration);
#pragma warning restore CA2000

        var settings = new ConnectionSettings(pool);

        if (elasticConfiguration.ConnectionType != "CloudConnectionPool" &&
            !string.IsNullOrWhiteSpace(elasticConfiguration.Username) &&
            !string.IsNullOrWhiteSpace(elasticConfiguration.Password))
        {
            settings.BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password);
        }

        if (!string.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
        }

        if (elasticConfiguration.EnableApiVersioningHeader)
        {
            settings.EnableApiVersioningHeader();
        }

        return settings;
    }

    private static IConnectionPool GetConnectionPool(ElasticConnectionOptions elasticConfiguration)
    {
        var uris = elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port.ToTechnicalString()}")).Distinct();
        IConnectionPool pool = null;
        switch (elasticConfiguration.ConnectionType)
        {
            case "SingleNodeConnectionPool":
                pool = new SingleNodeConnectionPool(uris.First());
                break;

            case "CloudConnectionPool":
                if (!string.IsNullOrWhiteSpace(elasticConfiguration.Username) &&
                    !string.IsNullOrWhiteSpace(elasticConfiguration.Password) &&
                    !string.IsNullOrWhiteSpace(elasticConfiguration.CloudId))
                {
                    using var credentials = new BasicAuthenticationCredentials(
                        elasticConfiguration.Username,
                        elasticConfiguration.Password);
                    pool = new CloudConnectionPool(elasticConfiguration.CloudId, credentials);
                }

                break;

            case "StaticConnectionPool":
                pool = new StaticConnectionPool(uris);
                break;

            case "SniffingConnectionPool":
                pool = new SniffingConnectionPool(uris);
                break;

            case "StickyConnectionPool":
                pool = new StickyConnectionPool(uris);
                break;

            default:
                pool = new SingleNodeConnectionPool(uris.First());
                break;
        }

        return pool;
    }
}
