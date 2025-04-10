using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenancePart : ContentPart
{
    public NumericField ProgressPercentage { get; } = new() { Value = 0 };
    public NumericField AllTenantCount { get; } = new() { Value = 0 };
    public NumericField ProcessedTenantsCount { get; } = new() { Value = 0 };
    public NumericField ProcessingStep { get; } = new() { Value = 1 };
    public NumericField CurrentVersion { get; } = new() { Value = 0 };

    public TimeField TimeSpanBetweenBatches { get; set; } = new() { Value = TimeSpan.FromSeconds(5) };

    public BooleanField Canceled { get; } = new();
    public BooleanField Running { get; } = new();

    public DateTimeField Started { get; set; } = new();
    public DateTimeField Finished { get; set; } = new();
    public IList<string> ProcessedTenantIds { get; } = [];
    public IDictionary<string, string> Versions { get; } = new Dictionary<string, string>();
    public IDictionary<string, string> ErrorLogs { get; } = new Dictionary<string, string>();

    public void AddErrorLog(string tenantName, string error) => ErrorLogs.Add(tenantName, error);

    public void AddVersion(string tenantName, string version) => Versions[tenantName] = version;

    public void CalculatePercentage() =>
        ProgressPercentage.Value = Math.Round((decimal)(ProcessedTenantsCount.Value / AllTenantCount.Value * 100)!, 0);

    public void SetVersion(bool newVersion)
    {
        if (newVersion || CurrentVersion.Value == 0)
        {
            CurrentVersion.Value++;
        }
    }

    public void ProcessTenant(string tenantId)
    {
        ProcessedTenantIds.Add(tenantId);
        ProcessedTenantsCount.Value++;
    }

    public void Clear(bool newVersion, bool reset)
    {
        if (newVersion || reset)
        {
            ProcessedTenantIds.Clear();
            AllTenantCount.Value = 0;
            ProcessedTenantsCount.Value = 0;
            ProgressPercentage.Value = 0;
            ErrorLogs.Clear();
            Finished.Value = null;
        }
    }

    public bool ShouldCancel(string jobId)
    {
        if (MaintenanceJobStore.IsCancelled(jobId))
        {
            Cancel();
            return true;
        }

        return false;
    }

    public void Cancel()
    {
        Canceled.Value = true;
        Running.Value = false;
    }

    public void Start(IClock clock, string maintenanceJobName, bool newVersion, bool reset)
    {
        MaintenanceJobStore.Clear(maintenanceJobName);
        Running.Value = true;
        Canceled.Value = false;
        Started.Value = clock.UtcNow;
        SetVersion(newVersion);
        Clear(newVersion, reset);
    }

    public void Finish(IClock clock)
    {
        Running.Value = false;
        Finished.Value = clock.UtcNow;
    }

    public bool IsFinished() => ProgressPercentage.Value == 100;

    public bool IsRunning() => ProgressPercentage.Value is > 0 and < 100 && !Canceled.Value;
}
