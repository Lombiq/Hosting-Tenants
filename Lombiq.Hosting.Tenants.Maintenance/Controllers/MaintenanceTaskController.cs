using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Controllers;

[Admin]
public class MaintenanceTaskController : Controller
{
    private readonly ISession _session;

    public MaintenanceTaskController(ISession session) =>
        _session = session;

    public async Task<IActionResult> Index()
    {
        var entities = await _session
            .Query<MaintenanceTaskExecutionData>(collection: DocumentCollections.Maintenance)
            .ListAsync(HttpContext.RequestAborted);

        return View(entities);
    }
}
