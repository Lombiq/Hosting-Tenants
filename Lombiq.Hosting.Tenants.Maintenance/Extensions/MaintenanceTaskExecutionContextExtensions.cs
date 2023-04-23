namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public static class MaintenanceTaskExecutionContextExtensions
{
    public static bool WasLatestExecutionSuccessful(this MaintenanceTaskExecutionContext execution) =>
        execution.LatestExecution?.IsSuccess == true;
}
