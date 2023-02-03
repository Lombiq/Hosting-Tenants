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

[BackgroundTask(Schedule = "* * * * *", Description = "Shut down idle tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var clock = serviceProvider.GetService<IClock>();
        var lastActiveTime = serviceProvider.GetService<ILastActiveTimeAccessor>().LastActiveDateTimeUtc;

        var maxIdleMinutes = serviceProvider.GetService<IOptions<IdleShutdownOptions>>().Value.MaxIdleMinutes;

        if (maxIdleMinutes <= 0) return;

        if (lastActiveTime.AddMinutes(maxIdleMinutes) <= clock?.UtcNow)
        {
            var shellSettings = serviceProvider.GetService<ShellSettings>();
            var logger = serviceProvider.GetService<ILogger<IdleShutdownTask>>();

            logger?.LogWarning("Shutting down tenant \"{ShellName}\" because of idle timeout.", shellSettings?.Name);

            var shellHost = serviceProvider.GetService<IShellHost>();

            try
            {
                await shellHost.ReleaseShellContextAsync(shellSettings);
            }
            catch (Exception e)
            {
                logger?.LogError(
                    e,
                    "Shutting down \"{ShellName}\" because of idle timeout failed with the following exception. Another shutdown will be attempted.",
                    shellSettings?.Name);

                // If the ReleaseShellContextAsync() fails (which can happen with a DB error: then the transaction
                // commits triggered by the dispose will fail) then while the tenant is unavailable the shell is still
                // active in a failed state. So first we need to correctly start the tenant, then shut it down for good.

                var shellSettingsManager = serviceProvider.GetService<IShellSettingsManager>();

                await InvokeRestartAsync(shellSettingsManager, shellHost, shellSettings);
                await shellHost.ReleaseShellContextAsync(shellSettings);
            }
        }
    }

    private static async Task InvokeRestartAsync(
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        await shellHost.UpdateShellSettingsAsync(shellSettings);
        await shellSettingsManager.SaveSettingsAsync(shellSettings);
    }
}
