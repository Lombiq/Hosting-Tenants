using Lombiq.Hosting.Tenants.Maintenance.Models;

namespace Lombiq.Hosting.Tenants.Maintenance.Extensions;

public static class MaintenanceTaskExecutionContextExtensions
{
    public static bool WasLatestExecutionSuccessful(this MaintenanceTaskExecutionContext execution) =>
        execution.LatestExecution?.IsSuccess == true;
}
