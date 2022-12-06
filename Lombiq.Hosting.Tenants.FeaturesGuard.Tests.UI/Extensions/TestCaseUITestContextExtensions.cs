using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestForbiddenFeaturesAsync(this UITestContext context, string setupRecipeId)
    {
        await SetUpNewTenantAndGoToFeaturesListAsync(context, setupRecipeId);

        // Ensure forbidden features are not available in the list.
        context.Missing(By.XPath("//label[@for='OrchardCore.Workflows.Session']"));
        context.Missing(By.XPath("//label[@for='OrchardCore.Search.Lucene']"));
        context.Missing(By.XPath("//label[@for='OrchardCore.MiniProfiler']"));
        context.Missing(By.XPath("//label[@for='Lombiq.Tests.UI.Shortcuts']"));
    }

    public static async Task TestConditionallyEnabledFeaturesAsync(this UITestContext context, string setupRecipeId)
    {
        // For a comprehensive summary of the feature activation and deactivation rules, see:
        // https://github.com/Lombiq/Hosting-Tenants/blob/dev/Lombiq.Hosting.Tenants.FeaturesGuard/Readme.md
        await SetUpNewTenantAndGoToFeaturesListAsync(context, setupRecipeId);

        // Lombiq's features that are set to always remain enabled from Manifest should have no disable button.
        context.Missing(By.XPath("//a[@id='btn-disable-Lombiq_Hosting_Tenants_Admin_Login_SubTenant']"));
        context.Missing(By.XPath("//a[@id='btn-disable-DotNest_Hosting_Tenants']"));
        context.Missing(By.XPath("//a[@id='btn-disable-DotNest_TenantsAdmin_Subtenant']"));

        // After setup, Twitter should be enabled.
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));

        // When ChartJs gets disabled but UIKit is enabled, Twitter should remain enabled.
        await context.ClickReliablyOnAsync(By.XPath("//a[@id='btn-disable-Lombiq_ChartJs']"));
        await context.ClickModalOkAsync();
        context.Exists(By.XPath("//a[@id='btn-disable-Lombiq_UIKit']"));
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));

        // When either UIKit or ChartJs is enabled, it should not be possible to disable Twitter.
        await context.ClickReliablyOnAsync(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));
        await context.ClickModalOkAsync();
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));

        // When UIKit gets disabled and ChartJs is also disabled, Twitter should get disabled.
        await context.ClickReliablyOnAsync(By.XPath("//a[@id='btn-disable-Lombiq_UIKit']"));
        await context.ClickModalOkAsync();
        context.Exists(By.XPath("//a[@id='btn-enable-Lombiq_ChartJs']"));
        context.Exists(By.XPath("//a[@id='btn-enable-OrchardCore_Twitter']"));

        // When UIKit is enabled, Twitter should get enabled.
        await context.ClickReliablyOnAsync(By.XPath("//a[@id='btn-enable-Lombiq_UIKit']"));
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));
    }

    private static async Task SetUpNewTenantAndGoToFeaturesListAsync(UITestContext context, string setupRecipeId)
    {
        await context.SignInDirectlyAsync();

        const string tenantName = "TestTenant";

        await context.CreateAndSwitchToTenantManuallyAsync(tenantName, "tt1", string.Empty, "features guard");

        await context.GoToSetupPageAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = tenantName,
                RecipeId = setupRecipeId,
                TablePrefix = tenantName,
                RunSetupOnCurrentPage = true,
            });

        await context.SignInDirectlyAsync();
        await context.GoToAdminRelativeUrlAsync("/Features");
    }
}
