using Lombiq.Hosting.BuildVersionDisplay.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using OrchardCore.Entities;
using System;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class MaintenanceTaskExecutionData : Entity
{
    public int Id { get; set; }
    public string MaintenanceId { get; set; }
    public DateTime ExecutionTimeUtc { get; set; }
    public DateTime? ExecutionEndUtc { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }
    public string BuildVersion { get; set; }
    public string OrchardVersion { get; set; }

    public static MaintenanceTaskExecutionData FromProvider(
        IMaintenanceProvider provider,
        DateTime utcNow)
    {
        var currentVersionModel = new BuildVersionModel();
        return new MaintenanceTaskExecutionData
        {
            MaintenanceId = provider.Id,
            ExecutionTimeUtc = utcNow,
            BuildVersion = currentVersionModel.BuildVersion,
            OrchardVersion = currentVersionModel.OrchardVersion,
        };
    }
}
