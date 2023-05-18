using Lombiq.Hosting.Tenants.SearchBotBlocker.Constants;
using Lombiq.Hosting.Tenants.SearchBotBlocker.Middlewares;
using Lombiq.Hosting.Tenants.SearchBotBlocker.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Tenants.FeaturesGuard;

[Feature(FeatureNames.SearchBotBlocker)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new SearchBotBlockerOptions();
        var configSection = _shellConfiguration
            .GetSection("Lombiq_Hosting_Tenants_SearchBotBlocker:SearchBotBlockerOptions");
        configSection.Bind(options);
        services.Configure<SearchBotBlockerOptions>(configSection);
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseMiddleware<SearchBotBlockerMiddleware>();
}
