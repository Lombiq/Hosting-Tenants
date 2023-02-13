using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Shut down idle tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    public IdleShutdownTask(
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
    }

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        _shellHost.ReleaseShellContextAsync(_shellSettings);
}
