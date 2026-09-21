using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Controllers;

public class AdminController : Controller
{
    private readonly ShellSettings _shellSettings;

    public AdminController(ShellSettings shellSettings) => _shellSettings = shellSettings;

    public async Task<IActionResult> Index()
    {
        // This page only makes sense for the default shell.
        if (!_shellSettings.IsDefaultShell()) return NotFound();
        
        // TODO Display unhealthy tenants. Acquire list from site setting and ping each to check if they are still unhealthy.
        return View();
    }
}
