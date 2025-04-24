namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenanceOptions
{
    public int? BatchIntervalSeconds { get; set; }
    public int? BatchSize { get; set; }
    public bool? RunParallel { get; set; }
}
