using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.StartStaggeredTenantWakeUp;

[Feature(FeatureNames.StaggeredTenantWakeUp)]
public sealed class Startup : StartupBase
{
    private readonly ShellSettings _shellSettings;

    public Startup(ShellSettings shellSettings) => _shellSettings = shellSettings;

    public override void ConfigureServices(IServiceCollection services)
    {
        if (!_shellSettings.IsDefaultShell()) return;

        services.AddScoped<IMaintenanceProvider, StartStaggeredTenantWakeUpProvider>();
    }
}
