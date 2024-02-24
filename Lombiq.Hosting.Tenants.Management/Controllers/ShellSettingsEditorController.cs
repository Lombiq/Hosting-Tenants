using Lombiq.HelpfulLibraries.Common.Utilities;
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
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Tenants.Permissions;

namespace Lombiq.Hosting.Tenants.Management.Controllers;

[Feature(FeatureNames.ShellSettingsEditor)]
public class ShellSettingsEditorController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
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
        if (JsonHelpers.ValidateJsonIfNotNull(model.Json) == false)
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
                shellSettings[tenantSettingsPrefixWithKey] = tenantConfiguration[key];
                shellSettings[key] = tenantConfiguration[key];
            }
        }

        var deletableKeys = currentSettings
            .Where(item => !tenantConfiguration.ContainsKey(item.Key))
            .Select(item => item.Key);

        foreach (var key in deletableKeys)
        {
            var tenantSettingsPrefixWithKey = $"{tenantSettingsPrefix}{key}";
            shellSettings[key] = null;
            shellSettings[tenantSettingsPrefixWithKey] = null;
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
