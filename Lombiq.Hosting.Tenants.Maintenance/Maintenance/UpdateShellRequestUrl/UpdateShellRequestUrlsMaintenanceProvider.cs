using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

public class UpdateShellRequestUrlsMaintenanceProvider(
    ShellSettings shellSettings,
    IOptions<UpdateShellRequestUrlMaintenanceOptions> options,
    IShellSettingsManager shellSettingsManager,
    IShellHost shellHost) : MaintenanceProviderBase
{
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(options.Value.IsEnabled &&
            shellSettings.IsDefaultShell() &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var allShellSettings = await shellSettingsManager.LoadSettingsAsync();
        foreach (var settings in allShellSettings)
        {
            settings.RequestUrlHost = TenantUrlHelpers.GetEvaluatedValueForTenant(
                options.Value.DefaultShellRequestUrl,
                options.Value.RequestUrl,
                settings);
            settings.RequestUrlPrefix = TenantUrlHelpers.GetEvaluatedValueForTenant(
                options.Value.DefaultShellRequestUrlPrefix,
                options.Value.RequestUrlPrefix,
                settings);

            await shellHost.UpdateShellSettingsAsync(settings);
        }

        context.ReloadShellAfterMaintenanceCompletion = true;
    }
}
