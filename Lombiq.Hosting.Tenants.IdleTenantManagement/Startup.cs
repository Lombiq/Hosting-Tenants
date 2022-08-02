using Lombiq.Hosting.Tenants.IdleTenantManagement.Constants;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Drivers;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Middlewares;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Navigation;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Permissions;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement;

[Feature(FeatureNames.DisableIdleTenants)]
public class DisableIdleTenantsStartup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public DisableIdleTenantsStartup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider) =>
            app.UseMiddleware<IdleTimeProviderMiddleware>();

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILastActiveTimeAccessor, LastActiveTimeAccessor>();
        services.AddSingleton<IBackgroundTask, IdleShutdownTask>();

        // Idle Minutes Settings
        services.Configure<IdleMinutesSettings>(
            _shellConfiguration.GetSection("Lombiq_Hosting_Tenants_IdleTenantManagement"));
        services.AddTransient<IConfigureOptions<IdleMinutesSettings>, IdleMinutesSettingsConfiguration>();
        services.AddScoped<IDisplayDriver<ISite>, IdleMinutesSettingsDisplayDriver>();
        services.AddScoped<IPermissionProvider, IdleMinutesPermissions>();
        services.AddScoped<INavigationProvider, IdleMinutesSettingsAdminMenu>();
    }
}
