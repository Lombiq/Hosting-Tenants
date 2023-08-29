using Lombiq.Hosting.Tenants.IdleTenantManagement.Constants;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Controllers;

[Authorize]
[Route("idle-tenant-test")]
[Feature(FeatureNames.ShutDownIdleTenants)]
public class IdleTenantTestController : Controller
{
    private readonly IIdleShutdown _idleShutdown;
    private readonly IShellHost _shellHost;
    private readonly IShellSettingsManager _shellSettingsManager;

    private static readonly HttpClient _httpClient = new();

    public IdleTenantTestController(
        IIdleShutdown idleShutdown,
        IShellHost shellHost,
        IShellSettingsManager shellSettingsManager)
    {
        _idleShutdown = idleShutdown;
        _shellHost = shellHost;
        _shellSettingsManager = shellSettingsManager;
    }

    public async Task<string> Index()
    {
        var shellSettings = (await _shellSettingsManager.LoadSettingsAsync()).First(shellSettings => shellSettings.Name == "allmodules");

        _httpClient.BaseAddress ??= new Uri(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host);

        for (int i = 0; i < 1000; i++)
        {
            await _shellHost.ReleaseShellContextAsync(shellSettings);

            await _httpClient.GetAsync(shellSettings.RequestUrlPrefix, HttpContext.RequestAborted);
        }

        return "OK";
    }

    [HttpGet("shutdown")]
    public async Task<string> ShutdownWithService()
    {
        await _idleShutdown.ShutDownIdleTenantsAsync();

        return "OK";
    }
}
