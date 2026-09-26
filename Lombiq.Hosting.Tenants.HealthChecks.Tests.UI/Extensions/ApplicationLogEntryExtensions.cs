using Lombiq.Hosting.Tenants.HealthChecks.Services;
using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Tests.UI.Extensions;

public static class ApplicationLogEntryExtensions
{
    /// <summary>
    /// Returns <see langword="false"/> if based on the <see cref="IApplicationLogEntry.Message"/> the log is emitted by
    /// the <see cref="HealthCheckService"/> or <see cref="ReportingHealthCheckService"/>.
    /// </summary>
    public static bool IsNotHealthCheckError(this IApplicationLogEntry logEntry) =>
        !logEntry.Message.Contains("Health check TestHealthCheck with status Unhealthy") &&
        !logEntry.Message.Contains("Tenant is Unhealthy:");
}
