using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.TenantUrlMaintenanceCore;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

public class UpdateShellRequestUrlsMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<TenantUrlMaintenanceOptions> _options;
    private readonly IShellSettingsManager _shellSettingsManager;

    public UpdateShellRequestUrlsMaintenanceProvider(
        ShellSettings shellSettings,
        IOptions<TenantUrlMaintenanceOptions> options,
        IShellSettingsManager shellSettingsManager)
    {
        _shellSettings = shellSettings;
        _options = options;
        _shellSettingsManager = shellSettingsManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_shellSettings.IsDefaultShell() && !context.WasLatestExecutionSuccessful());

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
