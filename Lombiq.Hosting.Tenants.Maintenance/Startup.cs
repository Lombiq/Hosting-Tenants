using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.Maintenance;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<StoreCollectionOptions>(o => o.Collections.Add(DocumentCollections.Maintenance));
        services.AddScoped<IDataMigration, Migrations>();
        services.AddSingleton<IIndexProvider, MaintenanceTaskExecutionIndexProvider>();

        // Temporarily commenting out.
        //// services.AddScoped<IModularTenantEvents, MaintenanceRunnerService>();
        //// services.AddScoped<IMaintenanceManager, MaintenanceManager>();
    }
}
