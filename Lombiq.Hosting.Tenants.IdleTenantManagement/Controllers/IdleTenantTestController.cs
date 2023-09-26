using Lombiq.Hosting.Tenants.IdleTenantManagement.Constants;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

    public async Task<string> Index(
        int releaseCount = 1000,
        string backupTenantName = "ikwileentaart",
        int shouldWait = 0)
    {
        var shellSettings = (await _shellSettingsManager.LoadSettingsAsync())
            .FirstOrDefault(tenantShellSettings => tenantShellSettings.Name == "princentest");
        shellSettings ??= (await _shellSettingsManager.LoadSettingsAsync())
            .First(tenantShellSettings => tenantShellSettings.Name == backupTenantName);
        var host = string.IsNullOrEmpty(shellSettings.RequestUrlHost)
            ? HttpContext.Request.Host.ToString()
            : shellSettings.RequestUrlHost;
        _httpClient.BaseAddress ??= new Uri(HttpContext.Request.Scheme + "://" + host);

        for (int i = 0; i < releaseCount; i++)
        {
            await _shellHost.ReleaseShellContextAsync(shellSettings);
            Wait(shouldWait);
            await _httpClient.GetAsync(shellSettings.RequestUrlPrefix, HttpContext.RequestAborted);
            Wait(shouldWait);
        }

        return "OK";
    }

    [HttpGet("shutdown")]
    public async Task<string> ShutdownWithService()
    {
        await _idleShutdown.ShutDownIdleTenantsAsync();

        return "OK";
    }

    private void Wait(int shouldWait)
    {
        if (shouldWait > 0)
        {
            Thread.Sleep(shouldWait);
        }
    }
}
