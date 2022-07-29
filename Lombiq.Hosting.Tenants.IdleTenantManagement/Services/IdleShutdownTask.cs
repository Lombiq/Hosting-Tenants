using Lombiq.Hosting.Tenants.IdleTenantManagement.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Disable Idle Tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var clock = serviceProvider.GetService<IClock>();
        var shellSettings = serviceProvider.GetService<ShellSettings>();
        var logger = serviceProvider.GetService<ILogger<IdleShutdownTask>>();
        var lastActiveTimeAccessor = serviceProvider.GetService<ILastActiveTimeAccessor>();
        var maxIdleMinutes = shellSettings.RuntimeQuotaSettings().MaxIdleMinutes;
        var shellSettingsManager = serviceProvider.GetService<IShellSettingsManager>();
        var shellHost = serviceProvider.GetService<IShellHost>();

        if (maxIdleMinutes <= 0) return;

        if (lastActiveTimeAccessor.LastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= clock.UtcNow)
        {
            logger.LogInformation("Shutting down tenant \"{ShellName}\" because of idle timeout", shellSettings.Name);

            try
            {
                await InvokeShutdownAsync(shellSettings, shellHost);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Shutting down \"{ShellName}\" because of idle timeout failed with the following exception. Another shutdown will be attempted",
                    shellSettings.Name);

                // If the ShellContext.Dispose() fails (which can happen with a DB error: then the transaction
                // commits triggered by the dispose will fail) then while the tenant is unavailable the shell is
                // still active in a failed state. So first we need to correctly start the tenant, then shut it
                // down for good.

                shellSettings.State = TenantState.Running;
                await InvokeRestartAsync(shellSettingsManager, shellHost, shellSettings);
                await InvokeShutdownAsync(shellSettings, shellHost);
            }
        }
    }

    private static Task InvokeShutdownAsync(ShellSettings shellSettings, IShellHost shellHost)
    {
        shellSettings.State = TenantState.Disabled;
        return shellHost.ReleaseShellContextAsync(shellSettings);
    }

    private static async Task InvokeRestartAsync(
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        await shellSettingsManager.SaveSettingsAsync(shellSettings);
        await shellHost.UpdateShellSettingsAsync(shellSettings);
    }
}
