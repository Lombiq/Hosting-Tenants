using OrchardCore.Entities;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class MaintenanceTaskExecutionData : Entity
{
    public int Id { get; set; }
    public string MaintenanceId { get; set; }
    public DateTime ExecutionTimeUtc { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }
}