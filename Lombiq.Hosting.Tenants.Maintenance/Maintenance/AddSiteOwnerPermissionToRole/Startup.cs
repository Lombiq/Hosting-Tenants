using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddSiteOwnerPermissionToRole;

[Feature(FeatureNames.AddSiteOwnerPermissionToRole)]
public class Startup(IShellConfiguration shellConfiguration) : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new AddSiteOwnerPermissionToRoleMaintenanceOptions();
        var configSection = shellConfiguration.GetSection("Lombiq_Hosting_Tenants_Maintenance:AddSiteOwnerPermissionToRole");
        configSection.Bind(options);
        services.Configure<AddSiteOwnerPermissionToRoleMaintenanceOptions>(configSection);

        services.AddScoped<IMaintenanceProvider, AddSiteOwnerPermissionToRoleMaintenanceProvider>();
    }
}
