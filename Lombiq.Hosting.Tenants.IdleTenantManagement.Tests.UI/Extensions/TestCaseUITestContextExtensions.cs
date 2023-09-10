using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
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

        var setupRecipeId = recipeId ?? DefaultIdleTenantSetupRecipeId;

        // Because this test is aimed at a single tenant's behavior we don't need dynamic tenant data.
        // The used constants here can be found at IdleTenantManagement.Tests.UI/Constants/IdleTenantData.
        await context.GoToSetupPageAndSetupOrchardCoreAsync(
            new OrchardCoreSetupParameters(context)
            {
                SiteName = IdleTenantName,
                RecipeId = setupRecipeId,
                TablePrefix = IdleTenantName,
                RunSetupOnCurrentPage = true,
            });

        await context.SignInDirectlyAsync();

        // We are letting the site to sit idle for more than two minutes so that the tenant could be shut down by the
        // background task.
        await Task.Delay(129420);

        // If we can access the admin menu after the tenant shut down that means the new shell was created and it is
        // working as intended.
        await context.SignInDirectlyAsync();
        await context.GoToDashboardAsync();
    }
}
