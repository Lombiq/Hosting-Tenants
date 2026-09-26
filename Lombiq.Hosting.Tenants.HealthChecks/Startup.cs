using Lombiq.Hosting.Tenants.HealthChecks.Constants;
using Lombiq.Hosting.Tenants.HealthChecks.Migrations;
using Lombiq.Hosting.Tenants.HealthChecks.Models;
using Lombiq.Hosting.Tenants.HealthChecks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace Lombiq.Hosting.Tenants.HealthChecks;

[Feature(HealthChecksFeatureIds.AllTenants)]
public sealed class HealthChecksTenantStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Decorate<HealthCheckService, ReportingHealthCheckService>();
        services.AddSingleton<IBackgroundTask, TenantCheckerBackgroundTask>();
    }
}

[Feature(HealthChecksFeatureIds.DefaultTenant)]
public sealed class HealthChecksDefaultStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<TenantHealthMigrations>();
        services.AddIndexProvider<TenantHealthIndexProvider>();
        services.AddHealthChecks().AddCheck<TenantHealthCheck>(nameof(TenantHealthCheck));
    }
}

[Feature(HealthChecksFeatureIds.Admin)]
public sealed class HealthChecksAdminStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<HealthChecksPermissions>();
        services.AddNavigationProvider<AdminMenu>();
    }
}
