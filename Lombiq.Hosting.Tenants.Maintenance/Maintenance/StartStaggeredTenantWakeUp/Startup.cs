using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Handlers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.StartStaggeredTenantWakeUp;

[Feature(FeatureNames.StaggeredTenantWakeUp)]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public Startup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        if (!_shellSettings.IsDefaultShell()) return;

        services.BindAndConfigureSection<StaggeredTenantWakeUpOptions>(
            _shellConfiguration,
            "Lombiq_Hosting_Tenants_Maintenance:StaggeredTenantWakeUp");

        services.AddContentPart<StaggeredTenantWakeUpPart>().WithMigration<StaggeredTenantWakeUpMigrations>();

        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IContentDisplayHandler, StaggeredTenantWakeUpDisplayHandler>();
        services.AddScoped<IContentHandler, StaggeredTenantWakeUpContentHandler>();
        services.AddScoped<IStaggeredTenantWakeUpService, StaggeredTenantWakeUpService>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<IMaintenanceProvider, StartStaggeredTenantWakeUpMaintenanceProvider>();
    }
}
