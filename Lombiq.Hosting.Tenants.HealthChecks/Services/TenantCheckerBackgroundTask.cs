using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Monitor tenant health.")]
public class TenantCheckerBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<HealthCheckService>();
        return service.CheckHealthAsync(cancellationToken);
    }
}
