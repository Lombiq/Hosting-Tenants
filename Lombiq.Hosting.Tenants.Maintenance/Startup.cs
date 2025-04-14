using Lombiq.HelpfulLibraries.Common.DependencyInjection;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Handlers;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;

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

[Feature(FeatureNames.StaggeredMaintenance)]
public sealed class StaggeredStartup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public StaggeredStartup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<StaggeredMaintenancePart>()
            .WithMigration<StaggeredMigrations>();
        services.AddScoped<IStaggeredMaintenanceService, StaggeredMaintenanceService>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IContentDisplayHandler, StaggeredMaintenanceHandler>();
        services.BindAndConfigureSection<StaggeredMaintenanceOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:StaggeredMaintenance");
    }
}
