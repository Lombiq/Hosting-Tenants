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
        var maxIdleMinutes = serviceProvider.GetRequiredService<IOptions<IdleShutdownOptions>>().Value.MaxIdleMinutes;

        var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

        if (maxIdleMinutes <= 0 || shellSettings.IsDefaultShell()) return;

        var clock = serviceProvider.GetRequiredService<IClock>();

        var lastActiveDateTimeUtc = serviceProvider.GetRequiredService<ILastActiveTimeAccessor>().LastActiveDateTimeUtc;

        if (lastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= clock?.UtcNow)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<IdleShutdownTask>>();

            logger?.LogWarning("Shutting down tenant \"{ShellName}\" because of idle timeout.", shellSettings.Name);

            var shellHost = serviceProvider.GetRequiredService<IShellHost>();

            await shellHost.ReleaseShellContextAsync(shellSettings, eventSource: false);
        }
    }
}
