using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer=System.Text.Json.JsonSerializer;

namespace Lombiq.Hosting.Tenants.Management.Services;

public class ReportingHealthCheckService : HealthCheckService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellSettings _shellSettings;

    public ReportingHealthCheckService(
        HealthCheckService healthCheckService,
        ILogger<ReportingHealthCheckService> logger,
        IServiceProvider serviceProvider,
        ShellSettings shellSettings)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _shellSettings = shellSettings;
    }

    public override async Task<HealthReport> CheckHealthAsync(
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        var report = await _healthCheckService.CheckHealthAsync(predicate, cancellationToken);

        if (report.Status == HealthStatus.Unhealthy)
        {
            if (_shellSettings.IsDefaultShell())
            {
                LogNotHealthy(_logger, report);
                await AddToNotHealthyTenantsAsync(_shellSettings.Name, report);
            }
            else
            {
                await _serviceProvider.WithShellScopeAsync(scope =>
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportingHealthCheckService>>();

                    LogNotHealthy(logger, report);
                    return AddToNotHealthyTenantsAsync(_shellSettings.Name, report);
                });
            }
        }

        return report;
    }

    private static void LogNotHealthy(ILogger logger, HealthReport report)
    {
        string json;
        try
        {
            json = JsonSerializer.Serialize(report);
        }
        catch (Exception exception)
        {
            json = JsonSerializer.Serialize(new
            {
                Failed = "Failed to serialize health report.",
                Error = exception.ToString(),
            });
        }

        logger.LogError("Tenant is {Status}: {Json}", report.Status, json);
    }

    private static async Task AddToNotHealthyTenantsAsync(string tenantName, HealthReport report)
    {
        // TODO save to a site setting.
    }
}
