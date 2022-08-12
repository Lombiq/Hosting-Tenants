using System;
using Fluid;
using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Lombiq.Hosting.MultiTenancy.Tenants.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.MultiTenancy.Tenants;

[Feature("FeaturesGuard")]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ForbiddenFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_MultiTenancy_Tenants:ForbiddenFeaturesOptions")
                .Bind(options));
        services.Configure<AlwaysOnFeaturesOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_MultiTenancy_Tenants:AlwaysOnFeaturesOptions")
                .Bind(options));
        services.AddScoped<IFeaturesGuardService, FeaturesGuardService>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
       //builder.UseMiddleware<FeaturesGuardService>();
    }
}
