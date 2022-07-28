using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Constants;
using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Middlewares;
using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime;

[Feature(FeatureNames.DisableIdleTenants)]
public class DisableIdleTenantsStartup : StartupBase
{
    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider) =>
        app.UseMiddleware<IdleTimeProviderMiddleware>();
    
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<IBackgroundTask, IdleShutdownTask>();
}
