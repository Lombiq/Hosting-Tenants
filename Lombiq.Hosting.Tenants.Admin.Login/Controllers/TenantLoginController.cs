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
using System.Linq;
using System.Threading.Tasks;
using static Lombiq.Hosting.Tenants.Admin.Login.Constants.FeatureNames;
using static Lombiq.Hosting.Tenants.Admin.Login.Constants.Roles;

namespace Lombiq.Hosting.Tenants.Admin.Login.Controllers;

[Feature(SubTenant)]
public class TenantLoginController(
    ISiteService siteService,
    SignInManager<IUser> userSignInManager,
    IShellHost shellHost,
    ShellSettings shellSettings,
    ILogger<TenantLoginController> logger,
    INotifier notifier,
    IHtmlLocalizer<TenantLoginController> htmlLocalizer) : Controller
{
    private readonly ILogger _logger = logger;
    private readonly IHtmlLocalizer H = htmlLocalizer;

    [HttpPost]
    // This is necessary because requests for this action will come from the Default tenant.
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index(string password)
    {
        if (shellSettings.Name.EqualsOrdinalIgnoreCase(ShellSettings.DefaultShellName))
        {
            return NotFound();
        }

        var defaultShell = await shellHost.GetScopeAsync(ShellSettings.DefaultShellName);
        var tenantLoginPasswordValidator = defaultShell?.ServiceProvider.GetService<ITenantLoginPasswordValidator>();

        if (defaultShell == null ||
            tenantLoginPasswordValidator == null ||
            !tenantLoginPasswordValidator.ValidatePassword(password))
        {
            return NotFound();
        }

        var sitesettings = await siteService.LoadSiteSettingsAsync();
        var adminUser = await userSignInManager.UserManager.FindByIdAsync(sitesettings.SuperUser);
        adminUser ??= (await userSignInManager.UserManager.GetUsersInRoleAsync(Administrator)).FirstOrDefault();

        if (adminUser == null)
        {
            await notifier.ErrorAsync(H["No user with administrator role in this tenant."]);
            return Redirect("~/");
        }

        await userSignInManager.SignInAsync(adminUser, isPersistent: false);
        _logger.LogInformation(1, "An admin user logged in from the Default tenant.");

        return RedirectToAction("Index", "Admin", new { area = "OrchardCore.Admin" });
    }
}
