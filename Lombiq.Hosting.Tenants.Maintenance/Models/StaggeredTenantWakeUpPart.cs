using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredTenantWakeUpPart : ContentPart
{
    public NumericField BatchSize { get; } = new() { Value = 1 };
    public NumericField BatchIntervalSeconds { get; set; } = new() { Value = 0 };
    public BooleanField RunParallel { get; set; } = new() { Value = true };
    public BooleanField RunOnStartup { get; set; } = new() { Value = true };
    public int CurrentVersion { get; set; }
    public int ProgressPercentage { get; set; }
    public int AllTenantCount { get; set; }
    public bool Paused { get; set; }
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public IList<string> ProcessedTenantIds { get; } = [];
    public IDictionary<string, string> Versions { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> ErrorLogs { get; } = new Dictionary<string, string>();

    public void CalculatePercentage() =>
        ProgressPercentage = (int)Math.Round(((double)ProcessedTenantIds.Count / AllTenantCount * 100)!, 0);

    public bool ShouldPause(string jobId)
    {
        if (MaintenanceJobStore.IsPaused(jobId))
        {
            Pause();
            return true;
        }

        return false;
    }

    public void Start(IClock clock, string maintenanceJobName, bool newVersion, bool reset)
    {
        MaintenanceJobStore.Clear(maintenanceJobName);
        Paused = false;
        Started = clock.UtcNow;
        Finished = null;
        SetVersion(newVersion);
        Clear(newVersion, reset);
    }

    public void Finish(IClock clock) => Finished = clock.UtcNow;

    public bool IsFinished() => ProgressPercentage == 100;

    public bool IsRunning() => ProgressPercentage is > 0 and < 100 && !Paused;

    private void SetVersion(bool newVersion)
    {
        if (newVersion || CurrentVersion == 0)
        {
            CurrentVersion++;
        }
    }

    private void Clear(bool newVersion, bool reset)
    {
        if (newVersion || reset)
        {
            ProcessedTenantIds.Clear();
            AllTenantCount = 0;
            ProgressPercentage = 0;
            ErrorLogs.Clear();
        }
    }

    private void Pause() => Paused = true;
}
