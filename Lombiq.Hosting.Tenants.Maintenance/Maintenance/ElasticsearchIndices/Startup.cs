using Elasticsearch.Net;
using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;
using System;
using System.Linq;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

[Feature(FeatureNames.DeleteOrRebuildElasticsearchIndices)]
public sealed class DeleteElasticsearchIndicesStartup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public DeleteElasticsearchIndicesStartup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.BindAndConfigureSection<ElasticsearchIndicesMaintenanceOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:ElasticsearchIndicesOptions");

        services.AddScoped<IMaintenanceProvider, DeleteElasticsearchIndicesMaintenanceProvider>();
        services.AddScoped<IMaintenanceProvider, RebuildElasticsearchIndicesMaintenanceProvider>();
    }
}

[Feature(FeatureNames.DeleteElasticsearchIndicesBeforeSetup)]
public sealed class DeleteElasticsearchIndicesBeforeSetupStartup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public DeleteElasticsearchIndicesBeforeSetupStartup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.BindAndConfigureSection<ElasticsearchIndicesMaintenanceOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:ElasticsearchIndicesOptions");

        // After the setup, the Elasticsearch module can be loaded the regular way.
        if (!_shellSettings.IsUninitialized()) return;

        // This is necessary to initialize Elasticsearch here like this, instead of using the Elasticsearch module from
        // OC. Because the Elasticsearch module can't be enabled the regular way if this module is added
        // as a setup feature, otherwise you get a ContentsAdminList shape missing exception on the admin dashboard.
        var configuration = _shellConfiguration.GetSection("OrchardCore_Elasticsearch");
        var elasticConfiguration = configuration.Get<ElasticConnectionOptions>();
        services.Configure<ElasticConnectionOptions>(options =>
            options.SetFileConfigurationExists(fileConfigurationExists: true));

        // Otherwise the ElasticClient won't work. Copied all this from the OC Elasticsearch module.
#pragma warning disable CA2000 // Call System. IDisposable. Dispose on object created by
        // 'GetConnectionSettings(elasticConfiguration)' before all references to it are out of scope
        var settings = GetConnectionSettings(elasticConfiguration);
#pragma warning restore CA2000

        services.AddSingleton<IElasticClient>(new ElasticClient(settings));
        DefaultElasticsearchIndexManager.AddDefaultServices(services);
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        if (!_shellSettings.IsUninitialized()) return;

        var options = serviceProvider.GetRequiredService<IOptions<ElasticsearchIndicesMaintenanceOptions>>().Value;
        if (options.BeforeSetupMiddlewareIsEnabled)
        {
            app.UseMiddleware<DeleteElasticsearchIndicesMiddleware>();
        }
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
                    // This is a copy of the OC Elasticsearch module's OrchardCore.Search.Elasticsearch.Startup.GetConnectionPool method.
#pragma warning disable CA2000 // CA2000: Call System. IDisposable. Dispose on object created by
                    // 'new BasicAuthenticationCredentials(' before all references to it are out of scope
                    var credentials = new BasicAuthenticationCredentials(
                        elasticConfiguration.Username,
                        elasticConfiguration.Password);
#pragma warning restore CA2000
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
