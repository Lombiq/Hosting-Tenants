using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class MaintenanceRunnerService : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;
    private readonly ILogger<MaintenanceRunnerService> _logger;
    private readonly Lazy<IMaintenanceManager> _maintenanceManagerLazy;

    public MaintenanceRunnerService(
        ShellSettings shellSettings,
        ILogger<MaintenanceRunnerService> logger,
        Lazy<IMaintenanceManager> maintenanceManagerLazy)
    {
        _shellSettings = shellSettings;
        _logger = logger;
        _maintenanceManagerLazy = maintenanceManagerLazy;
    }

    public override async Task ActivatedAsync()
    {
        if (_shellSettings.State != TenantState.Running) return;

        _logger.LogDebug(
            "Executing maintenance tasks on shell '{ShellName}'.",
            _shellSettings.Name);
        await _maintenanceManagerLazy.Value.ExecuteMaintenanceTasksAsync();
    }
}
