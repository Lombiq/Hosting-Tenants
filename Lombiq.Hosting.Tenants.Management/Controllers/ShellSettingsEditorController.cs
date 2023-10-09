using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(model.Json ?? string.Empty);
        var newSettings = new Dictionary<string, string>();

        var tenantSettingsPrefix = $"{model.TenantId}Prefix:";
        var currentSettings = shellSettings.ShellConfiguration.AsEnumerable()
            .Where(item => item.Value != null &&
                item.Key.Contains(tenantSettingsPrefix))
            .ToDictionary(key => key.Key.Replace(tenantSettingsPrefix, string.Empty), value => value.Value);

        if (settingsDictionary?.Keys != null)
        {
            foreach (var key in settingsDictionary.Keys)
            {
                var tenantSettingsPrefixWithKey = $"{model.TenantId}Prefix:{key}";
                if (shellSettings[key] != settingsDictionary[key])
                {
                    newSettings[tenantSettingsPrefixWithKey] = settingsDictionary[key];
                    newSettings[key] = settingsDictionary[key];
                }
            }
        }

        var deletableKeys = currentSettings
            .Where(item => settingsDictionary == null || !settingsDictionary.ContainsKey(item.Key))
            .Select(item => item.Key);

        foreach (var key in deletableKeys)
        {
            var tenantSettingsPrefixWithKey = $"{model.TenantId}Prefix:{key}";
            newSettings[key] = null;
            newSettings[tenantSettingsPrefixWithKey] = null;
        }

        var (locker, locked) = await _distributedLock.TryAcquireLockAsync("SHELL_SETTINGS_EDITOR_LOCK", TimeSpan.FromSeconds(10));
        if (!locked)
        {
            throw new TimeoutException($"Failed to acquire a lock before saving settings to the tenant: {model.TenantId}.");
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
