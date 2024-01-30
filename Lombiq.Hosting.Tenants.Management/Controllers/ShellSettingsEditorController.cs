using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Models;
using Lombiq.Hosting.Tenants.Management.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<ShellSettingsEditorController> H;

    public ShellSettingsEditorController(
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IShellConfigurationSources shellConfigurationSources,
        IDistributedLock distributedLock,
        INotifier notifier,
        IHtmlLocalizer<ShellSettingsEditorController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _shellConfigurationSources = shellConfigurationSources;
        _distributedLock = distributedLock;
        _notifier = notifier;
        H = htmlLocalizer;
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

        model.Json ??= "{}";
        if (!IsValidJson(model.Json))
        {
            await _notifier.ErrorAsync(H["Please provide valid JSON input for shell settings."]);
            TempData["ValidationErrorJson"] = model.Json;

            return RedirectToAction(
                nameof(AdminController.Edit),
                typeof(AdminController).ControllerName(),
                new
                {
                    area = "OrchardCore.Tenants",
                    id = model.TenantId,
                });
        }

        var tenantConfiguration = new JsonConfigurationParser().ParseConfiguration(model.Json);
        var newTenantConfiguration = new Dictionary<string, string>();

        var tenantSettingsPrefix = $"{model.TenantId}Prefix:";
        var currentSettings = shellSettings.ShellConfiguration.AsEnumerable()
            .Where(item => item.Value != null &&
                item.Key.Contains(tenantSettingsPrefix))
            .ToDictionary(key => key.Key.Replace(tenantSettingsPrefix, string.Empty), value => value.Value);

        foreach (var key in tenantConfiguration.Keys)
        {
            var tenantSettingsPrefixWithKey = $"{tenantSettingsPrefix}{key}";
            if (shellSettings[key] != tenantConfiguration[key])
            {
                newTenantConfiguration[tenantSettingsPrefixWithKey] = tenantConfiguration[key];
                newTenantConfiguration[key] = tenantConfiguration[key];
            }
        }

        var deletableKeys = currentSettings
            .Where(item => !tenantConfiguration.ContainsKey(item.Key))
            .Select(item => item.Key);

        foreach (var key in deletableKeys)
        {
            var tenantSettingsPrefixWithKey = $"{tenantSettingsPrefix}{key}";
            // We are using the shellSettings[key] directly because when we try to save it at line 109
            // it is not saving the new value. https://github.com/OrchardCMS/OrchardCore/issues/15184
            shellSettings[key] = null;
            shellSettings[tenantSettingsPrefixWithKey] = null;
        }

        var (locker, locked) =
            await _distributedLock.TryAcquireLockAsync(
                "LOMBIQ_HOSTING_TENANTS_MANAGEMENT_SHELL_SETTINGS_EDITOR_LOCK",
                TimeSpan.FromSeconds(10));

        if (!locked)
        {
            throw new TimeoutException($"Failed to acquire a lock before saving settings to the tenant: {model.TenantId}.");
        }

        await using var acquiredLock = locker;

        // We are using the shell configuration sources directly because using IShellHost.UpdateShellSettingsAsync would
        // not save settings that has a key with multiple sections, see
        // https://github.com/OrchardCMS/OrchardCore/issues/14481. Once this is fixed, we can get rid of the locking and
        // retrieve and save the shell settings settings with IShellHost.
        await _shellConfigurationSources.SaveAsync(shellSettings.Name, newTenantConfiguration);
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

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
