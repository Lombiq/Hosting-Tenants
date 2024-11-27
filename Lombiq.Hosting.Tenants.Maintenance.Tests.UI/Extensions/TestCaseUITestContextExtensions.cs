using Atata;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddAdministratorRoleToUsersWithRole;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Shouldly;
using System.Threading.Tasks;
using static OrchardCore.OrchardCoreConstants.Roles;

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
        await context.CreateUserAsync();
        await context.AddUserToRoleAsync(TestUser.UserName, Editor);

        await ResetMaintenanceAsync(context, nameof(AddAdministratorRoleToUsersWithRoleMaintenanceProvider));

        await context.Application.UsingScopeAsync(async serviceProvider =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IUser>>();
            var user = (User)await userManager.FindByNameAsync(TestUser.UserName);
            user.RoleNames.ShouldContain(Administrator);
        });
    }

    public static async Task ChangeUserSensitiveContentMaintenanceExecutionAsync(this UITestContext context)
    {
        await context.CreateUserAsync();
        await context.AddUserToRoleAsync(TestUser.UserName, Administrator);

        await ResetMaintenanceAsync(context, nameof(ChangeUserSensitiveContentMaintenanceProvider));

        await context.Application.UsingScopeAsync(async serviceProvider =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IUser>>();

            var testUser = (User)await userManager.FindByNameAsync(TestUser.UserName);
            testUser.UserName.ShouldBe(TestUser.UserName);
            testUser.Email.ShouldBe(TestUser.Email);

            (await userManager.FindByNameAsync(DefaultUser.UserName)).ShouldBeNull();
        });
    }

    private static async Task ResetMaintenanceAsync(UITestContext context, string maintenanceId)
    {
        await context.Application.UsingScopeAsync(async serviceProvider =>
        {
            var maintenanceManager = serviceProvider.GetRequiredService<IMaintenanceManager>();
            await maintenanceManager.DeleteMaintenanceExecutionsByIdAsync(maintenanceId);
        });

        await context.RestartAndWarmUpApplicationAsync();
    }
}
