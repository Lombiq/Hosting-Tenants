using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static Task TestMaintenanceExecutionAsync(this UITestContext context) => Task.CompletedTask;
}
