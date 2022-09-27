using Lombiq.Hosting.MultiTenancy.Tenants.Tests.UI.Constants;
using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestForbiddenFeaturesAsync(this UITestContext context)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();

        // Create new tenant manually.
        await context.CreateNewTenantManuallyAsync("Testenant", "tt1", "localhost", "features guard");

        // Setup newly created tenant.
        await context.ClickReliablyOnAsync(By.LinkText("Setup"));
        await context.ClickAndFillInWithRetriesAsync(By.Id("SiteName"), "Testenant");
        context.ExecuteScript(
            "document.querySelectorAll(\"a[data-recipe-name='" + Recipes.OsoceTestsRecipeId + "']\")[0].click()");
        await context.ClickAndFillInWithRetriesAsync(By.Id("UserName"), DefaultUser.UserName);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Email"), DefaultUser.Email);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Password"), DefaultUser.Password);
        await context.ClickAndFillInWithRetriesAsync(By.Id("PasswordConfirmation"), DefaultUser.Password);
        await context.ClickReliablyOnAsync(By.XPath("//button[@id='SubmitButton']"));

        // Log into tenant site and navigate to features list.
        await context.GoToRelativeUrlAsync("/tt1/admin/features");
        await context.ClickAndFillInWithRetriesAsync(By.Id("UserName"), DefaultUser.UserName);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Password"), DefaultUser.Password);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Log in')]"));

        // Ensure forbidden features are not available in the list.
        context.Missing(By.XPath("//label[@for='OrchardCore.Workflows.Session']"));
        context.Missing(By.XPath("//label[@for='OrchardCore.Lucene']"));
        context.Missing(By.XPath("//label[@for='OrchardCore.MiniProfiler']"));
        context.Missing(By.XPath("//label[@for='Lombiq.Tests.UI.Shortcuts']"));
    }

    public static async Task TestAlwaysEnabledFeaturesAsync(this UITestContext context)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();

        // Create new tenant manually.
        await context.CreateNewTenantManuallyAsync("Testenant", "tt1", "localhost", "features guard");

        // Setup newly created tenant.
        await context.ClickReliablyOnAsync(By.LinkText("Setup"));
        await context.ClickAndFillInWithRetriesAsync(By.Id("SiteName"), "Testenant");
        context.ExecuteScript(
            "document.querySelectorAll(\"a[data-recipe-name='" + Recipes.OsoceTestsRecipeId + "']\")[0].click()");
        await context.ClickAndFillInWithRetriesAsync(By.Id("UserName"), DefaultUser.UserName);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Email"), DefaultUser.Email);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Password"), DefaultUser.Password);
        await context.ClickAndFillInWithRetriesAsync(By.Id("PasswordConfirmation"), DefaultUser.Password);
        await context.ClickReliablyOnAsync(By.XPath("//button[@id='SubmitButton']"));

        // Log into tenant site and navigate to features list.
        await context.GoToRelativeUrlAsync("/tt1/admin/features");
        await context.ClickAndFillInWithRetriesAsync(By.Id("UserName"), DefaultUser.UserName);
        await context.ClickAndFillInWithRetriesAsync(By.Id("Password"), DefaultUser.Password);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(., 'Log in')]"));

        // Lombiq's features that are set to always remain enabled from Manifest should have no disable button.
        context.Missing(By.XPath("//a[@id='btn-disable-Lombiq_Hosting_Tenants_Admin_Login_SubTenant']"));
        context.Missing(By.XPath("//a[@id='btn-disable-DotNest_Hosting_Tenants']"));
        context.Missing(By.XPath("//a[@id='btn-disable-DotNest_TenantsAdmin_Subtenant']"));

        // Ensure trying to disable other always enabled features does not actually disable them.
        await context.ClickReliablyOnAsync(By.XPath("//a[@id='btn-disable-OrchardCore_Users']"));
        await context.ClickModalOkAsync();
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Users']"));
    }
}
