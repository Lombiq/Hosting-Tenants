using Lombiq.Hosting.Tenants.EnvironmentRobots.Constants;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Filters;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Middlewares;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots;

[Feature(FeatureNames.EnvironmentRobots)]
public class Startup(IShellConfiguration shellConfiguration) : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new EnvironmentRobotsOptions();
        var configSection = shellConfiguration
            .GetSection("Lombiq_Hosting_Tenants_EnvironmentRobots:EnvironmentRobotsOptions");
        configSection.Bind(options);
        services.Configure<EnvironmentRobotsOptions>(configSection);

        services.AddMvc(options => options.Filters.Add(typeof(EnvironmentRobotsMetaTagFilter)));
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseMiddleware<EnvironmentRobotsMiddleware>();
}
