using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Controllers;

public class ReleaseShellContextTestController : Controller
{
    private readonly IShellHost _shellHost;
    private readonly IShellSettingsManager _shellSettingsManager;

    private static readonly HttpClient _httpClient = new();

    public ReleaseShellContextTestController(IShellHost shellHost, IShellSettingsManager shellSettingsManager)
    {
        _shellHost = shellHost;
        _shellSettingsManager = shellSettingsManager;
    }

    public async Task<string> SendRequests()
    {
        var demoShellSettings = (await _shellSettingsManager.LoadSettingsAsync()).First(shellSettings => shellSettings.Name == "PsiProctor");

        _httpClient.BaseAddress ??= new Uri(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host);

        for (int i = 0; i < 1000; i++)
        {
            await _shellHost.ReleaseShellContextAsync(demoShellSettings);
            var response = await _httpClient.GetAsync(demoShellSettings.RequestUrlPrefix, HttpContext.RequestAborted);
        }

        return "OK";
    }
}
