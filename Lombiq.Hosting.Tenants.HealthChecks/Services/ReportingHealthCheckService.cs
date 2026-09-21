using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer=System.Text.Json.JsonSerializer;

namespace Lombiq.Hosting.Tenants.Management.Services;

public class ReportingHealthCheckService : HealthCheckService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger _logger;
    public ReportingHealthCheckService(
        HealthCheckService healthCheckService,
        ILogger<ReportingHealthCheckService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public override async Task<HealthReport> CheckHealthAsync(
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        var report = await _healthCheckService.CheckHealthAsync(predicate, cancellationToken);
        if (report.Status == HealthStatus.Healthy) return report;

        string json;
        try
        {
            json = JsonSerializer.Serialize(report);
        }
        catch (Exception e)
        {
            json = JsonSerializer.Serialize(new
            {
                Failed = "Cannot serialize.",
                Error = e.GetType().FullName,
                e.Message,
            });
        }

        _logger.LogError("Tenant is {Status}: {Json}", report.Status, json);

        return report;
    }
}
