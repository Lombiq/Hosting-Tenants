using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Services;

[BackgroundTask(Schedule = "0 * * * *", Description = "Check tenant health hourly.")]
public class TenantCheckerBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // TODO periodically run health checks using HealthCheckService on running tenants. Also trigger this when the tenant starts.
        throw new NotImplementedException();
    }
}
