using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Shut down idle tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var idleShutdown = serviceProvider.GetRequiredService<IIdleShutdown>();

        return idleShutdown.ShutDownIdleTenantsAsync();
    }
}
