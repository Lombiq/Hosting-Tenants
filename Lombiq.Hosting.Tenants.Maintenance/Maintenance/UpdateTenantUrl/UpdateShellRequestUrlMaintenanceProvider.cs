using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateTenantUrl;

public class UpdateShellRequestUrlMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<UpdateTenantUrlMaintenanceOptions> _options;
    private readonly IShellSettingsManager _shellSettingsManager;

    public UpdateShellRequestUrlMaintenanceProvider(
        ShellSettings shellSettings,
        IOptions<UpdateTenantUrlMaintenanceOptions> options,
        IShellSettingsManager shellSettingsManager)
    {
        _shellSettings = shellSettings;
        _options = options;
        _shellSettingsManager = shellSettingsManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        if (!_options.Value.Enabled || !_shellSettings.IsDefaultShell()) return Task.FromResult(false);

        return Task.FromResult(!context.WasLatestExecutionSuccessful());
    }

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var allShellSettings = await _shellSettingsManager.LoadSettingsAsync();
        foreach (var shellSettings in allShellSettings.Where(settings => settings.State == TenantState.Running))
        {
            string tenantUrl = TenantUrlHelper.GetTenantUrl(_options.Value, shellSettings);
            shellSettings.RequestUrlHost = tenantUrl;
            await _shellSettingsManager.SaveSettingsAsync(shellSettings);
        }

        context.ReloadShellAfterMaintenanceCompletion = true;
    }
}
