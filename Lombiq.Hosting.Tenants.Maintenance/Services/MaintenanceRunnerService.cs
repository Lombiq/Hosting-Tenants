using Microsoft.Extensions.DependencyInjection;
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
    private readonly IShellHost _shellHost;

    public MaintenanceRunnerService(
        ShellSettings shellSettings,
        ILogger<MaintenanceRunnerService> logger,
        IShellHost shellHost)
    {
        _shellSettings = shellSettings;
        _logger = logger;
        _shellHost = shellHost;
    }

    public override async Task ActivatedAsync()
    {
        if (_shellSettings.State != TenantState.Running) return;

        // Getting the scope here is important because the shell might not be fully initialized yet.
        var shellScope = await _shellHost.GetScopeAsync(_shellSettings.Name);
        var maintenanceManager = shellScope.ServiceProvider.GetService<IMaintenanceManager>();

        _logger.LogInformation(
            "Executing maintenance tasks on shell '{ShellName}'.",
            _shellSettings.Name);
        await maintenanceManager.ExecuteMaintenanceTasksAsync();
    }
}
