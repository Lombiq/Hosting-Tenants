using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Services;
using Lombiq.Tests.UI.Shortcuts.Controllers;
using Lombiq.Tests.UI.Shortcuts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenQA.Selenium;
using Shouldly;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    extension(UITestContext context)
    {
        public async Task TestHealthChecksAsync(string tenantSetupRecipeId)
        {
            context.Configuration.ResponseLogFilters["Ignore expected non-ok responses from ~/health/live"] = args =>
                !args.Response.Url.Contains("/health/live");

            // Temporarily disable HTML validation. All we do until this is re-enabled is interact with the
            // ~/Lombiq.Tests.UI.Shortcuts/Error/HealthCheck and ~/health/live pages, which return plain text. So we'd
            // just validate the browser's text renderer which is pointless.
            var htmlValidation = context.Configuration.HtmlValidationConfiguration.HtmlValidationAndAssertionOnPageChangeRule;
            context.Configuration.HtmlValidationConfiguration.HtmlValidationAndAssertionOnPageChangeRule = _ => false;

            // Create 3 tenants. The first and last are unhealthy using UITT's TestHealthCheck provider.
            await context.CreateAsync("test1", "UI Test 1", tenantSetupRecipeId);

            await context.CreateAsync("test2", "UI Test 2", tenantSetupRecipeId);
            await context.GoToAsync<ErrorController>(controller =>
                controller.HealthCheck(nameof(HealthStatus.Healthy), "UI Test 2"));
            context.HealthCheckShouldBe(HealthStatus.Healthy);

            await context.CreateAsync("test3", "UI Test 3", tenantSetupRecipeId);

            // Visit the Admin > Multi-Tenancy > Health Checks page and validate that test1 and test3 are listed with
            // the correct health check provider's reasons.
            context.SwitchCurrentTenantToDefault();
            await context.SignInDirectlyAndGoToDashboardAsync();
            context.Configuration.HtmlValidationConfiguration.HtmlValidationAndAssertionOnPageChangeRule = htmlValidation;
            await context.ClickThroughAdminMenuAsync(By.ClassName("menu-multitenancy"), By.Id("health-checks"));
            context.AssertUnhealthyTenants(
                "test1 TestHealthCheck: UI Test 1",
                "test3 TestHealthCheck: UI Test 3");

            // Flip test3 back to "Healthy" status.
            context.Application.UsingScopeServiceProviderAsync(
                provider =>
                {
                    provider.GetRequiredService<TestHealthCheckStatusAccessor>().Status = HealthStatus.Healthy;
                    return Task.CompletedTask;
                },
                "test3");

            // Upon page load, unhealthy tenants should be automatically re-checked. In this case the page in Default
            // pings test1 and test3 (but not the already-healthy test2), and find that the latter is healthy now.
            await context.RefreshAsync();
            context.AssertUnhealthyTenants("test1 TestHealthCheck: UI Test 1");
        }

        private async Task CreateAsync(
            string name,
            string unhealthyMessage,
            string tenantSetupRecipeId)
        {
            context.SwitchCurrentTenantToDefault();
            var parameters = new OrchardCoreSetupParameters(context, tenantSetupRecipeId);
            await context.CreateAndSwitchToTenantAsync(name, name, parameters);
            await context.GoToAsync<ErrorController>(controller =>
                controller.HealthCheck(nameof(HealthStatus.Unhealthy), unhealthyMessage));
            context.HealthCheckShouldBe(HealthStatus.Unhealthy);
        }

        private void HealthCheckShouldBe(HealthStatus status) =>
            context.Get(By.TagName("pre")).GetTextTrimmed().ShouldBe(status.ToString());

        private void AssertUnhealthyTenants(params string[] expected) =>
            context
                .GetAllWhenOneExists(By.CssSelector(".ta-content > ul > li"))
                .Select(element => Regex.Replace(element.Text.Trim(), @"\s+", " "))
                .ToArray()
                .ShouldBe(expected);
    }
}
