using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

public class UpdateShellRequestUrlsMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<UpdateShellRequestUrlMaintenanceOptions> _options;
    private readonly IShellSettingsManager _shellSettingsManager;

    public UpdateShellRequestUrlsMaintenanceProvider(
        ShellSettings shellSettings,
        IOptions<UpdateShellRequestUrlMaintenanceOptions> options,
        IShellSettingsManager shellSettingsManager)
    {
        _shellSettings = shellSettings;
        _options = options;
        _shellSettingsManager = shellSettingsManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.Value.IsEnabled &&
            _shellSettings.IsDefaultShell() &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var allShellSettings = await _shellSettingsManager.LoadSettingsAsync();
        foreach (var shellSettings in allShellSettings)
        {
            shellSettings.RequestUrlHost = TenantUrlHelpers.GetEvaluatedValueForTenant(
                _options.Value.DefaultShellRequestUrl,
                _options.Value.RequestUrl,
                shellSettings);
            shellSettings.RequestUrlPrefix = TenantUrlHelpers.GetEvaluatedValueForTenant(
                _options.Value.DefaultShellRequestUrlPrefix,
                _options.Value.RequestUrlPrefix,
                shellSettings);

            await _shellSettingsManager.SaveSettingsAsync(shellSettings);
        }

        context.ReloadShellAfterMaintenanceCompletion = true;
    }
}
