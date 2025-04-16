namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenanceOptions
{
    public int? TimeSpanMilliseconds { get; set; }
    public int? ProcessingStep { get; set; }
    public bool? RunParallel { get; set; }
}
