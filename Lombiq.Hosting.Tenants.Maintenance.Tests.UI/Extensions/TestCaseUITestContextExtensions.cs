using Atata;
using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestSiteUrlMaintenanceExecutionAsync(this UITestContext context)
    {
        await context.SignInDirectlyAsync();
        await context.GoToAdminRelativeUrlAsync("/Settings/general");
        context.Get(By.Name("ISite.BaseUrl")).GetValue().ShouldBe("https://test.com");
    }

    public static async Task TestSiteOwnerPermissionToRoleMaintenanceExecutionAsync(this UITestContext context)
    {
        await context.SignInDirectlyAsync();
        await context.GoToAdminRelativeUrlAsync("/Roles/Edit/Editor");
        context.Get(By.Id("Checkbox.SiteOwner")).GetDomProperty("checked").ShouldBe(bool.TrueString);
    }

    public static async Task ChangeUserSensitiveContentMaintenanceExecutionAsync(this UITestContext context)
    {
        const string username = "TestUser";
        const string lombiqUserCreatorRecipe = "Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Users";
        await context.ExecuteRecipeDirectlyAsync(lombiqUserCreatorRecipe);

        var loginPage = await context.GoToLoginPageAsync();
        (await loginPage.LogInWithAsync(context, username, DefaultUser.Password))
            .ShouldLeaveLoginPage();

        await context.GoToDashboardAsync();
        await context.GoToUsersAsync();

        context.Exists(By.XPath($"//h5[contains(text(), '{username}')]"));
        context.Exists(By.XPath($"//span[contains(text(), 'TestUser@lombiq.com')]"));
        context.Missing(By.XPath($"//h5[contains(text(), '{DefaultUser.UserName}')]"));
        context.Missing(By.XPath($"//span[contains(text(), '{DefaultUser.Email}')]"));
    }
}
