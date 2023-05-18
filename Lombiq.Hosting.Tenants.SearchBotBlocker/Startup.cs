using Lombiq.Hosting.Tenants.SearchBotBlocker.Constants;
using Lombiq.Hosting.Tenants.SearchBotBlocker.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Tenants.FeaturesGuard;

[Feature(FeatureNames.SearchBotBlocker)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IConfiguration configuration, IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseMiddleware<SearchBotBlockerMiddleware>();
}
