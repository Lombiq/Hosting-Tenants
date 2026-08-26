using Lombiq.Hosting.BuildVersionDisplay.Models;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using System;

namespace Lombiq.Hosting.Tenants.Maintenance.Extensions;

public static class MaintenanceTaskExecutionContextExtensions
{
    public static bool WasLatestExecutionSuccessful(this MaintenanceTaskExecutionContext execution) =>
        execution.LatestExecution?.IsSuccess == true;

    /// <summary>
    /// Returns <see langword="true"/> if the latest execution failed, or if it's from an older build version.
    /// </summary>
    public static bool IsFailedOrOutdated(this MaintenanceTaskExecutionContext execution) =>
        execution.LatestExecution is not { } latest ||
        !latest.IsSuccess ||
        new Version(latest.BuildVersion) < BuildVersionModel.AssemblyBuildVersion;
}
