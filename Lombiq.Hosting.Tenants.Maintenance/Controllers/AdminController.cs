using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

[Admin("StaggeredMaintenance/{action}")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<AdminController> H;
    private readonly IStaggeredMaintenanceService _staggeredMaintenanceService;
    private readonly Lazy<IContentManager> _contentManagerLazy;

    public delegate Task CompletionEventHandler(StaggeredMaintenancePart part);

    public static CompletionEventHandler OnComplete { get; set; }

    public AdminController(
        ILogger<AdminController> logger,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStaggeredMaintenanceService staggeredMaintenanceService,
        Lazy<IContentManager> contentManagerLazy)
    {
        _logger = logger;
        _notifier = notifier;
        H = htmlLocalizer;
        _staggeredMaintenanceService = staggeredMaintenanceService;
        _contentManagerLazy = contentManagerLazy;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> GetPartialView()
    {
        var model = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();

        return PartialView("StaggeredMaintenanceDetails", model);
    }

    public async Task<IActionResult> Start()
    {
        await ExecuteScheduledMaintenanceAsync();
        OnComplete += async part =>
        {
            if (part == null)
            {
                _logger.LogError("Maintenance failed!");
                await _notifier.ErrorAsync(H["Maintenance failed!"]);
                return;
            }
            _logger.LogError("Maintenance complete! Processed: {Count}", part.ProcessedTenantsCount.Value);
            await Task.CompletedTask;
        };

        await _notifier.SuccessAsync(H["Started staggered maintenance."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> NewVersion()
    {
        await ExecuteScheduledMaintenanceAsync(newVersion: true);

        await _notifier.SuccessAsync(H["Started staggered maintenance for new version."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> Reset()
    {
        await ExecuteScheduledMaintenanceAsync(reset: true);

        await _notifier.SuccessAsync(H["Started staggered maintenance with reset."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> Cancel()
    {
        var successfulCancel = MaintenanceJobStore.RequestCancel(nameof(StaggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync));

        // If not successful we should directly set the part to cancelled, because it is not running.
        if (!successfulCancel)
        {
            var staggeredMaintenance = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();
            staggeredMaintenance.Alter<StaggeredMaintenancePart>(part => part.Canceled.Value = true);
            await _contentManagerLazy.Value.UpdateAsync(staggeredMaintenance);
        }

        await _notifier.SuccessAsync(H["Cancelled staggered maintenance."]);
        return RedirectToIndex();
    }

    private RedirectToActionResult RedirectToIndex() =>
        RedirectToAction(nameof(Index));

    private Task ExecuteScheduledMaintenanceAsync(bool newVersion = false, bool reset = false) =>
        HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(nameof(AdminController), async scope =>
        {
            var staggeredMaintenanceService = scope.ServiceProvider.GetRequiredService<IStaggeredMaintenanceService>();
            var staggeredMaintenancePart = await staggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync(newVersion, reset);
            await OnComplete.InvokeAsync<CompletionEventHandler>(eventHandler => eventHandler(staggeredMaintenancePart));
        });
}
