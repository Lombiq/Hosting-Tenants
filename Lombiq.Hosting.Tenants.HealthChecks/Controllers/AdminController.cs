using Lombiq.Hosting.Tenants.HealthChecks.Constants;
using Lombiq.Hosting.Tenants.HealthChecks.Models;
using Lombiq.Hosting.Tenants.HealthChecks.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.HealthChecks.Controllers;

[Feature(HealthChecksFeatureIds.Admin)]
public class AdminController : Controller
{
    private readonly ISession _session;
    private readonly ShellSettings _shellSettings;

    public AdminController(
        ISession session,
        ShellSettings shellSettings)
    {
        _session = session;
        _shellSettings = shellSettings;
    }

    public async Task<IActionResult> Index()
    {
        // This page only makes sense for the default shell.
        if (!_shellSettings.IsDefaultShell()) return NotFound();

        var tenantHealthData = (await _session
                .Query<TenantHealth, TenantHealthIndex>(index => !index.IsHealthy)
                .ListAsync(HttpContext.RequestAborted))
            .AsList();

        // Re-check each unhealthy tenant to avoid misleading information.
        foreach (var index in tenantHealthData)
        {
            await HttpContext.RequestServices.WithShellScopeAsync(
                async scope =>
                {
                    var service = scope
                        .ServiceProvider
                        .GetServices<HealthCheckService>()
                        .CastWhere<ReportingHealthCheckService>()
                        .FirstOrDefault();

                    if (service == null) return;

                    var report = await service.CheckHealthAsync(predicate: null, HttpContext.RequestAborted);
                    index.IsHealthy = ReportingHealthCheckService.IsHealthy(report);
                },
                index.TenantName);
        }

        // Remove items that have been re-checked and found to be healthy.
        tenantHealthData.RemoveAll(index => index.IsHealthy);

        return View(tenantHealthData);
    }
}
