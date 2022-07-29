using Lombiq.Hosting.Tenants.IdleTenantManagement.Constants;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Middlewares;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement;

[Feature(FeatureNames.DisableIdleTenants)]
public class DisableIdleTenantsStartup : StartupBase
{
    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider) =>
            app.UseMiddleware<IdleTimeProviderMiddleware>();

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILastActiveTimeAccessor, LastActiveTimeAccessor>();
        services.AddSingleton<IBackgroundTask, IdleShutdownTask>();
    }
}
