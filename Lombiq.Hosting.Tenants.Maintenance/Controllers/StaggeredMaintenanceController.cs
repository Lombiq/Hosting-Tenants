using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

public class StaggeredMaintenanceController : Controller
{
    public async Task<IActionResult> Index()
    {
        await ExecuteScheduledMaintenanceAsync();

        return Ok("Maintenance tasks have been scheduled.");
    }

    public async Task<IActionResult> NewVersion()
    {
        await ExecuteScheduledMaintenanceAsync(newVersion: true);

        return Ok("Maintenance tasks have been scheduled.");
    }

    public async Task<IActionResult> Reset()
    {
        await ExecuteScheduledMaintenanceAsync(reset: true);

        return Ok("Maintenance tasks have been scheduled.");
    }

    public IActionResult Cancel()
    {
        MaintenanceJobStore.RequestCancel(nameof(StaggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync));
        return Ok("Cancellation requested.");
    }

    private Task ExecuteScheduledMaintenanceAsync(bool newVersion = false, bool reset = false) =>
        HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(nameof(StaggeredMaintenanceController), scope =>
        {
            var staggeredMaintenanceService = scope.ServiceProvider.GetRequiredService<IStaggeredMaintenanceService>();
            return staggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync(newVersion, reset);
        });
}
