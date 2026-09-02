using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

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
        services.AddScoped<IMaintenanceProvider, RebuildElasticsearchIndexesMaintenanceProvider>();
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
        // OC. Because the Elasticsearch module can't be enabled the regular way if this module is added as a setup
        // feature, otherwise you get a ContentsAdminList shape missing exception on the admin dashboard.
        services.AddSingleton(_ => _shellConfiguration.CreateElasticsearchClient());
        services.Configure<ElasticsearchConnectionOptions>(options =>
            options.SetFileConfigurationExists(fileConfigurationExists: true));

        services.AddSingleton<ElasticsearchIndexManager>();
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
}
