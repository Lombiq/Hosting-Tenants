using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        var shellSettingsManager = serviceProvider.GetService<IShellSettingsManager>();
        var shellHost = serviceProvider.GetService<IShellHost>();
        var options = serviceProvider.GetService<IOptions<IdleMinutesOptions>>();

        var maxIdleMinutes = options.Value.MaxIdleMinutes;

        if (maxIdleMinutes <= 0) return;

        if (lastActiveTimeAccessor.LastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= clock?.UtcNow)
        {
            logger?.LogError("Shutting down tenant \"{ShellName}\" because of idle timeout", shellSettings?.Name);

            try
            {
                await InvokeShutdownAsync(shellSettings, shellHost);
            }
            catch (Exception e)
            {
                logger?.LogError(
                    e,
                    "Shutting down \"{ShellName}\" because of idle timeout failed with the following exception. Another shutdown will be attempted",
                    shellSettings?.Name);

                // If the ReleaseShellContextAsync() fails (which can happen with a DB error: then the transaction
                // commits triggered by the dispose will fail) then while the tenant is unavailable the shell is
                // still active in a failed state. So first we need to correctly start the tenant, then shut it
                // down for good.

                await InvokeRestartAsync(shellSettingsManager, shellHost, shellSettings);
                await InvokeShutdownAsync(shellSettings, shellHost);
            }
        }
    }

    private static Task InvokeShutdownAsync(ShellSettings shellSettings, IShellHost shellHost) =>
        shellHost.ReleaseShellContextAsync(shellSettings);

    private static async Task InvokeRestartAsync(
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        await shellSettingsManager.SaveSettingsAsync(shellSettings);
        await shellHost.UpdateShellSettingsAsync(shellSettings);
    }
}
