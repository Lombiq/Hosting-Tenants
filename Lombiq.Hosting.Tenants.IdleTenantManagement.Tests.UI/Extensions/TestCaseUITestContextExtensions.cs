using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Constants.IdleTenantData;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestIdleTenantManagerBehaviorAsync(
        this UITestContext context,
        string recipeId = DefaultIdleTenantSetupRecipeId)
    {
        // Setting up new tenant to test the feature
        await context.CreateAndSwitchToTenantManuallyAsync(IdleTenantName, IdleTenantPrefix, string.Empty);

        // Because this test is aimed at a single tenant's behavior we don't need dynamic tenant data.
        // The used constants here can be found at IdleTenantManagement.Tests.UI/Constants/IdleTenantData.
        await context.GoToSetupAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = IdleTenantName,
                RecipeId = recipeId,
                TablePrefix = IdleTenantName,
                RunSetupOnCurrentPage = true,
            });

        // Due to the background ask scheduling configuration in ConfigureIdleTenantManagementTestSettings(), the
        // background task should run within not much more than a minute (background tasks are run with a frequency of
        // at least a minute, due to the limitation of cron expressions). Polling for it here.
        await Task.Delay(TimeSpan.FromMinutes(1), context.Configuration.TestCancellationToken);
        await context.DoWithRetriesOrFailAsync(
            async () =>
            {
                var logEntries = await context.Application.GetLogEntriesFromAllLogsAsync(context.Configuration.TestCancellationToken);
                return logEntries.Any(logEntry =>
                    logEntry.Message == $"Shutting down tenant \"{IdleTenantName}\" because of idle timeout.");
            },
            TimeSpan.FromMinutes(1),
            TimeSpan.FromSeconds(1));

        // If we can access the admin menu after the tenant shut down that means the new shell was created and it is
        // working as intended.
        await context.SignInDirectlyAsync();
        await context.GoToAdminAsync();
    }
}
