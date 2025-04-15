using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

[Admin("StaggeredMaintenance/{action}")]
public class AdminController : Controller
{
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<AdminController> H;
    private readonly IStaggeredMaintenanceService _staggeredMaintenanceService;
    private readonly IContentManager _contentManager;

    public AdminController(
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStaggeredMaintenanceService staggeredMaintenanceService,
        IContentManager contentManager)
    {
        _notifier = notifier;
        H = htmlLocalizer;
        _staggeredMaintenanceService = staggeredMaintenanceService;
        _contentManager = contentManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();
        return View(model: model);
    }

    public async Task<IActionResult> GetPartialView()
    {
        var model = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();

        return PartialView("StaggeredMaintenanceDetails", model);
    }

    public async Task<IActionResult> Continue()
    {
        await ExecuteScheduledMaintenanceAsync();

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

        // If not successful we should directly set the part to cancelled, because it is not running. This could happen
        // if the maintenance was abruptly stopped e.g. by a server restart.
        if (!successfulCancel)
        {
            var staggeredMaintenance = await _staggeredMaintenanceService.GetorCreateStaggeredMaintenanceAsync();
            staggeredMaintenance.Alter<StaggeredMaintenancePart>(part => part.Canceled.Value = true);
            await _contentManager.UpdateAsync(staggeredMaintenance);
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
            await staggeredMaintenanceService.RunScheduledMaintenanceForAllTenantAsync(newVersion, reset);
        });
}
