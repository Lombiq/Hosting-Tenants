using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestIdleTenantManagerBehaviorAsync(this UITestContext context)
    {
        // We are letting the site to sit idle for more than two minutes so that the
        // tenant could be shut down by the background task.
        await Task.Delay(129420);

        // If we can access the admin menu after the tenant shut down that means the new shell was created
        // and it is working as intended.
        await context.SignInDirectlyAsync();
        await context.GoToDashboardAsync();
    }
}
