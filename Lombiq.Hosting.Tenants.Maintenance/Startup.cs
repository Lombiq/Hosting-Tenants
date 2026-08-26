using Lombiq.HelpfulLibraries.Common.DependencyInjection;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddLazyInjectionSupport();
        services.Configure<StoreCollectionOptions>(options => options.Collections.Add(DocumentCollections.Maintenance));

        services.AddIndexProvider<MaintenanceTaskExecutionIndexProvider>();
        services.AddDataMigration<Migrations>();

        services.AddScoped<IModularTenantEvents, MaintenanceRunnerService>();
        services.AddScoped<IMaintenanceManager, MaintenanceManager>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<MaintenancePermissions>();
    }
}

[RequireFeatures("OrchardCore.Elasticsearch")]
public sealed class ElasticsearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddDataMigration<QueryElasticsearchMigration>();
}
