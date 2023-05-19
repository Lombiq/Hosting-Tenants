namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class MaintenanceTaskExecutionContext
{
    public MaintenanceTaskExecutionData LatestExecution { get; set; }
    public MaintenanceTaskExecutionData CurrentExecution { get; set; }
    public bool ReloadShellAfterMaintenanceCompletion { get; set; }
}
