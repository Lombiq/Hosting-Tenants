using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.DeleteElasticsearchIndices;

[Feature(FeatureNames.DeleteElasticsearchIndices)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public Startup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.BindAndConfigureSection<DeleteElasticsearchIndicesMaintenanceOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:DeleteElasticsearchIndices");

        services.AddScoped<IMaintenanceProvider, DeleteElasticsearchIndicesMaintenanceProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        if (!_shellSettings.IsUninitialized()) return;

        var options = serviceProvider.GetRequiredService<IOptions<DeleteElasticsearchIndicesMaintenanceOptions>>().Value;
        if (options.IsEnabled)
        {
            app.UseMiddleware<DeleteElasticsearchIndicesMiddleware>();
        }
    }
}
