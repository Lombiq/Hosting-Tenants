using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Tenants.Permissions;

namespace Lombiq.Hosting.Tenants.Management.Controllers;

[Feature(FeatureNames.ShellSettingsEditor)]
public class ShellSettingsEditorController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly IShellConfigurationSources _shellConfigurationSources;
    private readonly IDistributedLock _distributedLock;

    public ShellSettingsEditorController(
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IShellConfigurationSources shellConfigurationSources,
        IDistributedLock distributedLock)
    {
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _shellConfigurationSources = shellConfigurationSources;
        _distributedLock = distributedLock;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShellSettingsEditorViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ManageTenants) ||
            !_shellHost.TryGetSettings(model.TenantId, out var shellSettings))
        {
            return NotFound();
        }

        var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(model.Json);
        var newSettings = new Dictionary<string, string>();

        foreach (var key in settingsDictionary.Keys.Where(key => string.IsNullOrEmpty(settingsDictionary[key])))
        {
            settingsDictionary[key] = null;
        }

        foreach (var key in settingsDictionary.Keys)
        {
            if (shellSettings[key] != settingsDictionary[key])
            {
                var tenantSettingsPrefix = $"{model.TenantId}Prefix:{key}";
                newSettings[tenantSettingsPrefix] = settingsDictionary[key];
                newSettings[key] = settingsDictionary[key];
            }
        }

        // Try to acquire a lock before using the scope, so that a next process gets the last committed data.
        var (locker, locked) = await _distributedLock.TryAcquireLockAsync("SHELL_SETTINGS_EDITOR_LOCK", TimeSpan.MaxValue);
        if (!locked)
        {
            throw new TimeoutException($"Failed to acquire a lock before saving settings to the tenant: {model.TenantId}");
        }

        await using var acquiredLock = locker;

        await _shellConfigurationSources.SaveAsync(shellSettings.Name, newSettings);
        await _shellHost.ReloadShellContextAsync(shellSettings);

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
