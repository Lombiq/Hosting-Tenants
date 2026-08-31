using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

[Admin]
public class MaintenanceTaskController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IClock _clock;
    private readonly IMaintenanceManager _maintenanceManager;
    private readonly INotifier _notifier;
    private readonly ISession _session;

    private readonly IHtmlLocalizer<MaintenanceTaskController> H;

    public MaintenanceTaskController(
        IAuthorizationService authorizationService,
        IClock clock,
        IMaintenanceManager maintenanceManager,
        INotifier notifier,
        ISession session,
        IHtmlLocalizer<MaintenanceTaskController> localizer)
    {
        _authorizationService = authorizationService;
        _clock = clock;
        _maintenanceManager = maintenanceManager;
        _notifier = notifier;
        _session = session;

        H = localizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, MaintenancePermissions.ViewMaintenanceTaskExecutions))
        {
            return NotFound();
        }

        var entities = await _session
            .Query<MaintenanceTaskExecutionData>(collection: DocumentCollections.Maintenance)
            .ListAsync(HttpContext.RequestAborted);

        return View(entities);
    }

    public async Task<IActionResult> Start(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, MaintenancePermissions.StartMaintenanceTaskExecutions) ||
            _maintenanceManager.GetProviderById(id) is not { } provider)
        {
            return NotFound();
        }

        var context = new MaintenanceTaskExecutionContext
        {
            LatestExecution = await _maintenanceManager.GetLatestExecutionByMaintenanceIdAsync(provider.Id),
            CurrentExecution = MaintenanceTaskExecutionData.FromProvider(provider, _clock.UtcNow),
        };

        var result = await _maintenanceManager.ExecuteMaintenanceTaskIfNeededAsync(provider, context, forceExecute: true);
        switch (result?.IsSuccess)
        {
            case null:
                await _notifier.ErrorAsync(
                    H["The \"{0}\" maintenance task's provider did not start. This should not be possible.", id]);
                break;
            case true:
                var seconds = (result.ExecutionEndUtc - result.ExecutionTimeUtc)?.TotalSeconds;
                await _notifier.SuccessAsync(
                    H["The \"{0}\" maintenance task ran to completion in {1:0.##} seconds.", id, seconds]);
                break;
            default:
                await _notifier.ErrorAsync(
                    H["The \"{0}\" maintenance task failed with the following error: {1}", id, result.Error]);
                break;
        }

        return RedirectToAction(nameof(Index));
    }
}
