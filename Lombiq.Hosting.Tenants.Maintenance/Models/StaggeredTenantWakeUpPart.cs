using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredTenantWakeUpPart : ContentPart
{
    public NumericField BatchSize { get; } = new() { Value = 1 };
    public TimeField BatchInterval { get; set; } = new() { Value = TimeSpan.FromSeconds(0) };
    public BooleanField RunParallel { get; set; } = new() { Value = true };
    public NumericField ProgressPercentage { get; } = new() { Value = 0 };
    public NumericField AllTenantCount { get; } = new() { Value = 0 };
    public NumericField CurrentVersion { get; } = new() { Value = 0 };
    public BooleanField Paused { get; } = new();
    public DateTimeField Started { get; set; } = new();
    public DateTimeField Finished { get; set; } = new();
    public IList<string> ProcessedTenantIds { get; } = [];
    public IDictionary<string, string> Versions { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> ErrorLogs { get; } = new Dictionary<string, string>();

    public TimeSpan GetOptionsTimeBetweenBatches(StaggeredTenantWakeUpOptions options) =>
        options.BatchIntervalSeconds is null
            ? BatchInterval.Value!.Value
            : TimeSpan.FromSeconds(options.BatchIntervalSeconds.Value);

    public decimal GetOptionsBatchSize(StaggeredTenantWakeUpOptions options) =>
        options.BatchSize ?? BatchSize.Value!.Value;

    public bool GetOptionsRunParallel(StaggeredTenantWakeUpOptions options) =>
        options.RunParallel ?? RunParallel.Value;

    public void CalculatePercentage() =>
        ProgressPercentage.Value = Math.Round((decimal)(ProcessedTenantIds.Count / AllTenantCount.Value * 100)!, 0);

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
        Paused.Value = false;
        Started.Value = clock.UtcNow;
        Finished.Value = null;
        SetVersion(newVersion);
        Clear(newVersion, reset);
    }

    public void Finish(IClock clock) => Finished.Value = clock.UtcNow;

    public bool IsFinished() => ProgressPercentage.Value == 100;

    public bool IsRunning() => ProgressPercentage.Value is > 0 and < 100 && !Paused.Value;

    private void SetVersion(bool newVersion)
    {
        if (newVersion || CurrentVersion.Value == 0)
        {
            CurrentVersion.Value++;
        }
    }

    private void Clear(bool newVersion, bool reset)
    {
        if (newVersion || reset)
        {
            ProcessedTenantIds.Clear();
            AllTenantCount.Value = 0;
            ProgressPercentage.Value = 0;
            ErrorLogs.Clear();
        }
    }

    private void Pause() => Paused.Value = true;
}
