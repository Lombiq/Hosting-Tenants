using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class MaintenanceRunnerService(
    ShellSettings shellSettings,
    ILogger<MaintenanceRunnerService> logger,
    Lazy<IMaintenanceManager> maintenanceManagerLazy) : ModularTenantEvents
{
    public override async Task ActivatedAsync()
    {
        if (shellSettings.State != TenantState.Running) return;

        logger.LogDebug(
            "Executing maintenance tasks on shell '{ShellName}'.",
            shellSettings.Name);
        await maintenanceManagerLazy.Value.ExecuteMaintenanceTasksAsync();
    }
}
