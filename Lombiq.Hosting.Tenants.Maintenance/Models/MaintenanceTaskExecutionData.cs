using Lombiq.Hosting.BuildVersionDisplay.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using OrchardCore.Entities;
using System;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class MaintenanceTaskExecutionData : Entity
{
    private const string WarningPrefix = "WARN:";

    public int Id { get; set; }
    public string MaintenanceId { get; set; }
    public DateTime ExecutionTimeUtc { get; set; }
    public DateTime? ExecutionEndUtc { get; set; }

    public bool IsSuccess
    {
        get => !string.IsNullOrEmpty(Error);

        [Obsolete($"In future versions this setter will be removed, making {nameof(IsSuccess)} a get-only property.")]
        set
        {
            if (value != string.IsNullOrEmpty(Error))
            {
                Error = value ? "Unknown Error" : null;
            }
        }
    }

    public bool IsWarning => Error?.StartsWithOrdinal(WarningPrefix) == true;
    public string Error { get; set; }
    public string BuildVersion { get; set; }
    public string OrchardVersion { get; set; }

    public void SetWarning(string text) =>
        Error = $"{WarningPrefix} {text}";

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
