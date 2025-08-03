using Atata;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddAdministratorRoleToUsersWithRole;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Lombiq.Tests.UI.Constants;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OrchardCore.Environment.Shell;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.OrchardCoreConstants.Roles;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    private const string TenantName = "StaggeredTenantWakeUp";
    private const string TenantUrlPrefix = "staggered-maintenance";

    public static async Task TestStaggeredTenantWakeUpAsync(this UITestContext context, string setupRecipeId)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();
        await context.CreateAndSwitchToTenantManuallyAsync(TenantName, TenantUrlPrefix, string.Empty);

        await context.GoToSetupAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = TenantName,
                RecipeId = setupRecipeId,
                TablePrefix = TenantName,
                RunSetupOnCurrentPage = true,
            });

        context.SwitchCurrentTenant(tenantName: null, string.Empty);

        await context.SignInDirectlyAndGoToDashboardAsync();
        await context.ClickReliablyOnByLinkTextAsync("Multi-Tenancy");
        await context.ClickReliablyOnByLinkTextAsync("Tenants");
        await context.ClickReliablyOnByLinkTextAsync("StaggeredTenantWakeUp");
        await context.GoToAdminAsync();

        // Put the tenant into idle mode, to see if it works that way also.
        await context.Application.UsingScopeAsync(
            async serviceProvider =>
            {
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var shellHost = serviceProvider.GetRequiredService<IShellHost>();
                await shellHost.ReleaseShellContextAsync(shellSettings, eventSource: false);
            },
            TenantName);

        await context.ClickReliablyOnByLinkTextAsync("Staggered Tenant Wake-Up");
        await context.ClickReliablyOnByLinkTextAsync("Start new");

        await context.DoWithRetriesOrFailAsync(
            async () =>
            {
                await context.ClickReliablyOnByLinkTextAsync("Staggered Tenant Wake-Up");
                return context.Get(By.XPath("//div[@id='maintenance-progress-bar']/..")).Text == "100 %";
            },
            TimeSpan.FromMinutes(1),
            TimeSpan.FromSeconds(1));

        await context.ClickReliablyOnAsync(By.XPath("//button[contains(.,'Versions')]"));
        context.Get(By.XPath(
            $"//tbody//td[contains(.,'{TenantName}')]/../td[contains(.,'1')]/../td[contains(.,'Edit')]"));

        // Confirm that the tenant is sleeping again after the maintenance.
        await context.Application.UsingScopeAsync(serviceProvider =>
            {
                var shellHost = serviceProvider.GetRequiredService<IShellHost>();
                var allRunningTenants = shellHost.GetAllSettings().Where(shell => !shell.IsDefaultShell() && shell.IsRunning());
                allRunningTenants.Where(tenantSettings =>
                    shellHost.TryGetShellContext(tenantSettings.Name, out var shellContext) && shellContext.IsActivated)
                    .ShouldBeEmpty();
                return Task.CompletedTask;
            });
    }

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
