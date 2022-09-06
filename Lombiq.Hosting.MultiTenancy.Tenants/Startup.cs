using Lombiq.Hosting.MultiTenancy.Tenants.Handlers;
using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Lombiq.Hosting.MultiTenancy.Tenants.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.MultiTenancy.Tenants;

[Feature("FeaturesGuard")]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AlwaysEnabledFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_MultiTenancy_Tenants:AlwaysEnabledFeaturesOptions")
                .Bind(options));

        services.AddScoped<IFeaturesGuardService, FeaturesGuardService>();
        services.AddScoped<IFeatureEventHandler, FeaturesEventHandler>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
       //app.UseMiddleware<FeaturesGuardService>();
    }
}
