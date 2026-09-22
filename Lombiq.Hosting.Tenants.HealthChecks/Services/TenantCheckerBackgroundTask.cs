using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Services;

[BackgroundTask(Schedule = "0 * * * *", Description = "Check tenant health hourly.")]
public class TenantCheckerBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<HealthCheckService>();
        return service.CheckHealthAsync(cancellationToken);
    }
}
