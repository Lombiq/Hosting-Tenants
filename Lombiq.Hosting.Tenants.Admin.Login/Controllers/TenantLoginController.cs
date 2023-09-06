using Lombiq.Hosting.Tenants.Admin.Login.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Lombiq.Hosting.Tenants.Admin.Login.Constants.FeatureNames;
using static Lombiq.Hosting.Tenants.Admin.Login.Constants.Roles;

namespace Lombiq.Hosting.Tenants.Admin.Login.Controllers;

[Feature(SubTenant)]
public class TenantLoginController : Controller
{
    private readonly ISiteService _siteService;
    private readonly SignInManager<IUser> _userSignInManager;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    public TenantLoginController(
        ISiteService siteService,
        SignInManager<IUser> userSignInManager,
        IShellHost shellHost,
        ShellSettings shellSettings,
        ILogger<TenantLoginController> logger,
        INotifier notifier,
        IHtmlLocalizer<TenantLoginController> htmlLocalizer)
    {
        _siteService = siteService;
        _userSignInManager = userSignInManager;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _logger = logger;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    [HttpPost]
    // This is necessary because requests for this action will come from the Default tenant.
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index(string password)
    {
        if (_shellSettings.Name.EqualsOrdinalIgnoreCase(ShellSettings.DefaultShellName))
        {
            return NotFound();
        }

        var defaultShell = await _shellHost.GetScopeAsync(ShellSettings.DefaultShellName);
        var tenantLoginPasswordValidator = defaultShell?.ServiceProvider.GetService<ITenantLoginPasswordValidator>();

        if (defaultShell == null ||
            tenantLoginPasswordValidator == null ||
            !tenantLoginPasswordValidator.ValidatePassword(password))
        {
            return NotFound();
        }

        var sitesettings = await _siteService.LoadSiteSettingsAsync();
        var adminUser = await _userSignInManager.UserManager.FindByIdAsync(sitesettings.SuperUser);
        adminUser ??= (await _userSignInManager.UserManager.GetUsersInRoleAsync(Administrator)).FirstOrDefault();

        if (adminUser == null)
        {
            await _notifier.ErrorAsync(H["No user with administrator role in this tenant."]);
            return Redirect("~/");
        }

        await _userSignInManager.SignInAsync(adminUser, isPersistent: false);
        _logger.LogInformation(1, "An admin user logged in from the Default tenant.");

        return RedirectToAction("Index", "Admin", new { area = "OrchardCore.Admin" });
    }
}
