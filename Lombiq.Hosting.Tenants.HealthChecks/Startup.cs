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

public sealed class TenantHealthChecksStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Decorate<HealthCheckService, ReportingHealthCheckService>();
        services.AddPermissionProvider<HealthChecksPermissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSingleton<IBackgroundTask, TenantCheckerBackgroundTask>();
        services.AddDataMigration<TenantHealthMigrations>();
        services.AddIndexProvider<TenantHealthIndexProvider>();
        services.AddHealthChecks().AddCheck<TenantHealthCheck>(nameof(TenantHealthCheck));
    }
}
