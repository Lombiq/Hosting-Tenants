using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Models;
using Lombiq.Hosting.Tenants.Management.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Controllers;

[Feature(FeatureNames.ShellSettingsEditor)]
public class ShellSettingsEditorController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly IShellsSettingsSources _settingsSources;
    private readonly SemaphoreSlim _tenantConfigSemaphore = new(1);
    private readonly IShellConfigurationSources _tenantConfigSources;

    public ShellSettingsEditorController(
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IShellsSettingsSources settingsSources,
        IShellConfigurationSources tenantConfigSources)
    {
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _settingsSources = settingsSources;
        _tenantConfigSources = tenantConfigSources;
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
        var newSettings = new Dictionary<string, string>();
        foreach (var key in settingsDictionary.Keys)
        {
            var newKey = key.Replace($"{model.TenantId}:", string.Empty, StringComparison.InvariantCulture);
            newSettings[newKey] = settingsDictionary[key];
        }

        await _tenantConfigSemaphore.WaitAsync(HttpContext.RequestAborted);
        try
        {
            //await _settingsSources.SaveAsync(shellSettings.Name, newSettings);
            await _tenantConfigSources.SaveAsync(shellSettings.Name, settingsDictionary);
        }
        finally
        {
            _tenantConfigSemaphore.Release();
        }

        await _shellHost.ReloadShellContextAsync(shellSettings);

        //await _shellHost.UpdateShellSettingsAsync(shellSettings);
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
