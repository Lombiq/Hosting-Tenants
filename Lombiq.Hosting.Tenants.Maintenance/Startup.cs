using Lombiq.HelpfulLibraries.Common.DependencyInjection;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddLazyInjectionSupport();
        services.Configure<StoreCollectionOptions>(options => options.Collections.Add(DocumentCollections.Maintenance));
        services.AddContentPart<StaggeredMaintenancePart>();
        services.AddContentPart<StaggeredMaintenanceTenantStatusPart>();

        services.AddScoped<IDataMigration, Migrations>();
        services.AddSingleton<IIndexProvider, MaintenanceTaskExecutionIndexProvider>();
        services.AddSingleton<IIndexProvider, StaggeredMaintenanceIndexProvider>();

        services.AddContentPart<StaggeredMaintenancePart>()
            .WithIndex<StaggeredMaintenanceIndexProvider>();
        services.AddContentPart<StaggeredMaintenanceTenantStatusPart>();
        services.AddScoped<IModularTenantEvents, MaintenanceRunnerService>();
        services.AddScoped<IMaintenanceManager, MaintenanceManager>();
        services.AddScoped<IStaggeredMaintenanceService, StaggeredMaintenanceService>();
    }
}
