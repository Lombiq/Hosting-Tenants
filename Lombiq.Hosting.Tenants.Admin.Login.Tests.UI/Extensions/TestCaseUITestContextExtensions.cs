using Atata;
using Lombiq.Hosting.Tenants.Admin.Login.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestTenantAdminLoginBehaviorAsync(this UITestContext context)
    {
        const string tenantName = "test";
        const string tenantUserName = "tenantAdmin";

        // Enable "Lombiq Hosting - Tenants Admin Login" feature (which indirectly enables the "Tenants" feature too).
        await context.EnableFeatureDirectlyAsync(FeatureNames.Module);

        // Create a tenant and turn on the "Lombiq Hosting - Tenants Admin Login Sub-tenant" feature in it.
        await context.CreateAndSwitchToTenantAsync(
            name: tenantName,
            urlPrefix: tenantName,
            new OrchardCoreSetupParameters(context) { SiteName = "Test Tenant Title", UserName = tenantUserName },
            // It's already enabled.
            enableTenantsFeature: false);
        await context.EnableFeatureDirectlyAsync(FeatureNames.SubTenant);

        // Go to the new tenant's edit page and click on the "Login as admin user" button.
        context.SwitchCurrentTenantToDefault();
        await context.SignInDirectlyAndGoToAdminRelativeUrlAsync("/Tenants/Edit/" + tenantName);
        await context.ClickReliablyOnAsync(By.CssSelector(".btn-success.log-in-as-admin"));
        context.SwitchToLastWindow();

        // Verify that we have landed in the tenant's admin menu.
        context.Driver.Url.ShouldContain($"/{tenantName}{context.AdminUrlPrefix}");
        context.Exists(By.XPath("//h4[contains(., 'Welcome to Orchard Core')]"));
        context.Exists(By.XPath("//a[contains(@class, 'navbar-brand')]//span[contains(., 'Test Tenant Title')]"));
        context
            .Get(By.CssSelector(".navbar .fa-user[data-bs-original-title]"))
            .GetAttribute("data-bs-original-title")
            .ShouldBe(tenantUserName);
    }
}
