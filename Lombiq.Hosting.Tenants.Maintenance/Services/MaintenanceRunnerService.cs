using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class MaintenanceRunnerService : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;
    private readonly ILogger<MaintenanceRunnerService> _logger;
    private readonly IMaintenanceManager _maintenanceManager;

    public MaintenanceRunnerService(
        ShellSettings shellSettings,
        ILogger<MaintenanceRunnerService> logger,
        IMaintenanceManager maintenanceManager)
    {
        _shellSettings = shellSettings;
        _logger = logger;
        _maintenanceManager = maintenanceManager;
    }

    public override async Task ActivatedAsync()
    {
        if (_shellSettings.State != TenantState.Running) return;

        // Getting the scope here is important because the shell might not be fully initialized yet.
        _logger.LogInformation(
            "Executing maintenance tasks on shell '{ShellName}'.",
            _shellSettings.Name);
        await _maintenanceManager.ExecuteMaintenanceTasksAsync();
    }
}
