using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using System;
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

        await RunTestConditionallyEnabledFeaturesAssertionsAsync(
            context,
            featureId => context.ClickReliablyOnAsync(By.XPath($"//a[@id='btn-enable-{featureId.Replace('.', '_')}']")),
            async featureId =>
            {
                await context.ClickReliablyOnAsync(By.XPath($"//a[@id='btn-disable-{featureId.Replace('.', '_')}']"));
                await context.ClickModalOkAsync();
            });

        // Doing the same checks but with recipes. Starting with a new tenant to make sure that the starting state is
        // correct.
        context.SwitchCurrentTenantToDefault();
        await SetUpNewTenantAndGoToFeaturesListAsync(context, setupRecipeId, "TestTenant2", "testtenant2");

        // Note that when doing feature operations via recipes, we deliberately use the JSON Import feature, not the
        // ExecuteRecipeDirectlyAsync() shortcut. This way, we can make sure that it works the same as it would for a
        // user.
        await RunTestConditionallyEnabledFeaturesAssertionsAsync(
            context,
            featureId => EnableFeatureViaJsonImportAndGoToFeaturesListAsync(context, featureId),
            featureId => DisableFeatureViaJsonImportAndGoToFeaturesListAsync(context, featureId));
    }

    private static async Task SetUpNewTenantAndGoToFeaturesListAsync(
        UITestContext context,
        string setupRecipeId,
        string tenantName = "TestTenant1",
        string tenantUrlPrefix = "testtenant1")
    {
        await context.SignInDirectlyAsync();

        await context.CreateAndSwitchToTenantManuallyAsync(tenantName, tenantUrlPrefix, string.Empty, "features guard");

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

    private static async Task RunTestConditionallyEnabledFeaturesAssertionsAsync(
        UITestContext context,
        Func<string, Task> enableFeature,
        Func<string, Task> disableFeature)
    {
        // When ChartJs gets disabled but UIKit is enabled, Twitter should remain enabled.
        await disableFeature("Lombiq.ChartJs");
        context.Exists(By.XPath("//a[@id='btn-disable-Lombiq_UIKit']"));
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));

        // When either UIKit or ChartJs is enabled, it should not be possible to disable Twitter.
        await disableFeature("OrchardCore.Twitter");
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));

        // When UIKit gets disabled and ChartJs is also disabled, Twitter should get disabled.
        await disableFeature("Lombiq.UIKit");
        context.Exists(By.XPath("//a[@id='btn-enable-Lombiq_ChartJs']"));
        context.Exists(By.XPath("//a[@id='btn-enable-OrchardCore_Twitter']"));

        // When UIKit is enabled, Twitter should get enabled.
        await enableFeature("Lombiq.UIKit");
        context.Exists(By.XPath("//a[@id='btn-disable-OrchardCore_Twitter']"));
    }

    private static Task EnableFeatureViaJsonImportAndGoToFeaturesListAsync(UITestContext context, string featureId) =>
        RunFeatureStepViaJsonImportAndGoToFeaturesListAsync(context, "enable", featureId);

    private static Task DisableFeatureViaJsonImportAndGoToFeaturesListAsync(UITestContext context, string featureId) =>
        RunFeatureStepViaJsonImportAndGoToFeaturesListAsync(context, "disable", featureId);

    private static async Task RunFeatureStepViaJsonImportAndGoToFeaturesListAsync(
        UITestContext context, string command, string featureId)
    {
        await context.GoToAdminRelativeUrlAsync("/DeploymentPlan/Import/Json");

        await context.FillInCodeMirrorEditorWithRetriesAsync(
            By.CssSelector(".CodeMirror.cm-s-default"),
            @"{ ""steps"": [ { ""name"": ""Feature"", """ + command + @""": [ """ + featureId + @""" ] } ] }");

        await context.ClickReliablyOnSubmitAsync();
        await context.GoToAdminRelativeUrlAsync("/Features");
    }
}
