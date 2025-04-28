using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

[Feature(FeatureNames.StaggeredTenantWakeUp)]
[Admin("StaggeredTenantWakeUp/{action}")]
public class AdminController : Controller
{
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<AdminController> H;
    private readonly IStaggeredTenantWakeUpService _staggeredTenantWakeUpService;
    private readonly IContentManager _contentManager;

    public AdminController(
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStaggeredTenantWakeUpService staggeredTenantWakeUpService,
        IContentManager contentManager)
    {
        _notifier = notifier;
        H = htmlLocalizer;
        _staggeredTenantWakeUpService = staggeredTenantWakeUpService;
        _contentManager = contentManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _staggeredTenantWakeUpService.GetOrCreateStaggeredTenantWakeUpAsync();
        return View(model: model);
    }

    public async Task<IActionResult> GetPartialView()
    {
        var model = await _staggeredTenantWakeUpService.GetOrCreateStaggeredTenantWakeUpAsync();

        return PartialView("StaggeredTenantWakeUpDetails", model);
    }

    public async Task<IActionResult> Continue()
    {
        await ExecuteStaggeredTenantWakeUpAsync();

        await _notifier.SuccessAsync(H["Started staggered tenant wake-up."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> NewVersion()
    {
        await ExecuteStaggeredTenantWakeUpAsync(newVersion: true);

        await _notifier.SuccessAsync(H["Started staggered tenant wake-up for new version."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> Reset()
    {
        await ExecuteStaggeredTenantWakeUpAsync(reset: true);

        await _notifier.SuccessAsync(H["Started staggered tenant wake-up with reset."]);
        return RedirectToIndex();
    }

    public async Task<IActionResult> Pause()
    {
        var successfulPause = MaintenanceJobStore.RequestPause(nameof(StaggeredTenantWakeUpService.RunScheduledMaintenanceForAllTenantAsync));

        // If not successful we should directly set the part to paused, because it is not running. This could happen
        // if the maintenance was abruptly stopped e.g. by a server restart.
        if (!successfulPause)
        {
            var staggeredTenantWakeUp = await _staggeredTenantWakeUpService.GetOrCreateStaggeredTenantWakeUpAsync();
            staggeredTenantWakeUp.Alter<StaggeredTenantWakeUpPart>(part => part.Paused = true);
            await _contentManager.UpdateAsync(staggeredTenantWakeUp);
        }

        await _notifier.SuccessAsync(H["Paused staggered tenant wake-up."]);
        return RedirectToIndex();
    }

    private RedirectToActionResult RedirectToIndex() =>
        RedirectToAction(nameof(Index));

    private Task ExecuteStaggeredTenantWakeUpAsync(bool newVersion = false, bool reset = false) =>
        StaggeredTenantWakeUpHelper.ExecuteStaggeredTenantWakeUpAsync(nameof(AdminController), newVersion, reset);
}
