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
    public DateTime? StartedUtc { get; set; }
    public DateTime? FinishedUtc { get; set; }
    public IList<string> ProcessedTenantIds { get; } = [];
    public IDictionary<string, string> Versions { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> ErrorLogs { get; private set; } = new Dictionary<string, string>();

    public void CalculatePercentage() =>
        ProgressPercentage = (int)Math.Round(((double)ProcessedTenantIds.Count / AllTenantCount * 100)!, 0, MidpointRounding.AwayFromZero);

    public bool ShouldPause(string jobId)
    {
        if (MaintenanceJobStore.IsPaused(jobId))
        {
            Pause();
            return true;
        }

        return false;
    }

    public void Start(IClock clock, string maintenanceJobName, bool newVersion)
    {
        MaintenanceJobStore.Clear(maintenanceJobName);
        Paused = false;
        StartedUtc = clock.UtcNow;
        FinishedUtc = null;
        SetVersion(newVersion);
        Clear(newVersion);
    }

    public void Finish(IClock clock) => FinishedUtc = clock.UtcNow;

    public bool IsFinished() => ProgressPercentage == 100;

    public bool IsRunning() => ProgressPercentage is > 0 and < 100 && !Paused;

    private void SetVersion(bool newVersion)
    {
        if (newVersion || CurrentVersion == 0)
        {
            CurrentVersion++;
        }
    }

    private void Clear(bool newVersion)
    {
        if (newVersion)
        {
            ProcessedTenantIds.Clear();
            AllTenantCount = 0;
            ProgressPercentage = 0;

            // This is needed because the dictionary won't be cleared with a simple Clear() and .Apply().
            ErrorLogs = null;
            this.Apply();
            ErrorLogs = new Dictionary<string, string>();
        }
    }

    private void Pause() => Paused = true;
}
