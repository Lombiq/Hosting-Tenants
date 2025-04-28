using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Helpers;

public static class StaggeredTenantWakeUpHelper
{
    public static Task ExecuteStaggeredTenantWakeUpAsync(string jobName, bool newVersion = false, bool reset = false) =>
        HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(jobName, async scope =>
        {
            var staggeredTenantWakeUpService = scope.ServiceProvider.GetRequiredService<IStaggeredTenantWakeUpService>();
            await staggeredTenantWakeUpService.RunScheduledMaintenanceForAllTenantAsync(newVersion, reset);
        });
}
