using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

public class StaggeredMaintenanceController : Controller
{
    private readonly ILogger<StaggeredMaintenanceController> _logger;

    public delegate Task CompletionEventHandler(StaggeredMaintenancePart part);

    public static CompletionEventHandler OnComplete { get; set; }

    public StaggeredMaintenanceController(ILogger<StaggeredMaintenanceController> logger) => _logger = logger;

    public async Task<IActionResult> Index()
    {
        await ExecuteScheduledMaintenanceAsync();
        OnComplete += async part =>
        {
            _logger.LogError("Maintenance complete! Processed: {Count}", part.ProcessedTenantsCount.Value);
            await Task.CompletedTask;
        };

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
        HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(nameof(StaggeredMaintenanceController), async scope =>
        {
            var staggeredMaintenanceService = scope.ServiceProvider.GetRequiredService<IStaggeredMaintenanceService>();
            var staggeredMaintenancePart = await staggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync(newVersion, reset);
            await OnComplete.InvokeAsync<CompletionEventHandler>(eventHandler => eventHandler(staggeredMaintenancePart));
        });
}
