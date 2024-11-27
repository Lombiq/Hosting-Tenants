using Atata;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddAdministratorRoleToUsersWithRole;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OrchardCore;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Shouldly;
using System;
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

    public static async Task TestAdministratorRoleToUsersWithRoleMaintenanceExecutionAsync(this UITestContext context)
    {
        // Preparing a user account with the Editor role. The user should get the Administrator role once the
        // maintenance runs.
        const string userName = "TestUser";
        await context.CreateUserAsync(userName, DefaultUser.Password, "testuser@example.com");
        await context.AddUserToRoleAsync(userName, "Editor");

        // Resetting the maintenance and restarting the app to let the it run.
        await context.Application.UsingScopeAsync(async serviceProvider =>
        {
            var maintenanceManager = serviceProvider.GetRequiredService<IMaintenanceManager>();
            await maintenanceManager
                .DeleteMaintenanceExecutionsByIdAsync(nameof(AddAdministratorRoleToUsersWithRoleMaintenanceProvider));
        });

        await context.RestartAndWarmUpApplicationAsync();

        await context.Application.UsingScopeAsync(async serviceProvider =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IUser>>();
            var user = (User)await userManager.FindByNameAsync(userName);
            user.RoleNames.ShouldContain(OrchardCoreConstants.Roles.Administrator);
        });
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
