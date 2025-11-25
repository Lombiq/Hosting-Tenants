using Lombiq.HelpfulLibraries.Common.DependencyInjection;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Handlers;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;

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
    }
}
}
