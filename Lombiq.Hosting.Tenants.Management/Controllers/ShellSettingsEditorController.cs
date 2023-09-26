using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Models;
using Lombiq.Hosting.Tenants.Management.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Controllers;

[Feature(FeatureNames.ShellSettingsEditor)]
public class ShellSettingsEditorController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;

    public ShellSettingsEditorController(
        IAuthorizationService authorizationService,
        IShellHost shellHost)
    {
        _authorizationService = authorizationService;
        _shellHost = shellHost;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShellSettingsEditorViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ShellSettingsEditPermissions.ShellSettingsEditPermission))
        {
            return Forbid();
        }

        if (!_shellHost.TryGetSettings(model.TenantId, out var shellSettings))
        {
            return NotFound();
        }

        var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(model.Json);
        foreach (var key in settingsDictionary.Keys)
        {
            shellSettings[key] = settingsDictionary[key];
        }

        await _shellHost.UpdateShellSettingsAsync(shellSettings);
        return RedirectToAction(
            nameof(AdminController.Edit),
            typeof(AdminController).ControllerName(),
            new
            {
                area = "OrchardCore.Tenants",
                id = model.TenantId,
            });
    }
}
