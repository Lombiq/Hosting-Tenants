using Lombiq.Hosting.Tenants.HealthChecks.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Lombiq.Hosting.Tenants.HealthChecks.Services;

public class ReportingHealthCheckService : HealthCheckService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellSettings _shellSettings;

    public ReportingHealthCheckService(
        HealthCheckService healthCheckService,
        IServiceProvider serviceProvider,
        ShellSettings shellSettings)
    {
        _healthCheckService = healthCheckService;
        _serviceProvider = serviceProvider;
        _shellSettings = shellSettings;
    }

    public async override Task<HealthReport> CheckHealthAsync(
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        var report = await _healthCheckService.CheckHealthAsync(predicate, cancellationToken);
        var tenantName = _shellSettings.Name;

        if (_shellSettings.IsDefaultShell())
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportingHealthCheckService>>();
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            await UpdateTenantHealthAsync(logger, session, tenantName, report, cancellationToken);
        }
        else
        {
            await _serviceProvider.WithShellScopeAsync(scope =>
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportingHealthCheckService>>();
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                return UpdateTenantHealthAsync(logger, session, tenantName, report, cancellationToken);
            });
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

    private static async Task UpdateTenantHealthAsync(
        ILogger logger,
        ISession session,
        string tenantName,
        HealthReport report,
        CancellationToken cancellationToken)
    {
        var tenantHealth = await session
            .Query<TenantHealth, TenantHealthIndex>(index => index.TenantName == tenantName)
            .FirstOrDefaultAsync(cancellationToken) ?? new TenantHealth { TenantName = tenantName };

        tenantHealth.IsHealthy = IsHealthy(report);
        tenantHealth.Report.SetItems(report
            .Entries
            .Where(pair => IsHealthy(pair.Value.Status))
            .ToDictionary(pair => pair.Key, pair => pair.Value.Description));

        if (!tenantHealth.IsHealthy)
        {
            LogNotHealthy(logger, report);
        }

        await session.SaveAsync(tenantHealth);
        await session.SaveChangesAsync(cancellationToken);
    }

    public static bool IsHealthy(HealthStatus status) => status != HealthStatus.Unhealthy;
    public static bool IsHealthy(HealthReport report) => IsHealthy(report.Status);
}
