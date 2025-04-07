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
        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(nameof(StaggeredMaintenanceController), scope =>
        {
            var staggeredMaintenanceService = scope.ServiceProvider.GetService<IStaggeredMaintenanceService>();

            // This method fetches the next batch of emails from the IMAP server. When it syncs an email it calls the
            // EmailSyncedAsync method of the registered email sync event handlers.
            return staggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync();
        });

        return Ok("Maintenance tasks have been scheduled.");
    }
}
