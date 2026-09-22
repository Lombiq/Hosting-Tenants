using Lombiq.Hosting.Tenants.HealthChecks.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Environment.Shell;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.HealthChecks.Services;

public class TenantHealthCheck : IHealthCheck
{
    private readonly ISession _session;
    private readonly ShellSettings _shellSettings;

    public TenantHealthCheck(ISession session, ShellSettings shellSettings)
    {
        _session = session;
        _shellSettings = shellSettings;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        _shellSettings.Name == ShellSettings.DefaultShellName
            ? CheckDefaultTenantHealthAsync(cancellationToken)
            : Task.FromResult(HealthCheckResult.Healthy());

    private async Task<HealthCheckResult> CheckDefaultTenantHealthAsync(CancellationToken cancellationToken)
    {
        var unhealthyTenants = (await _session
                .Query<TenantHealth, TenantHealthIndex>(index => !index.IsHealthy)
                .ListAsync(cancellationToken))
            .Select(item => item.TenantName)
            .ToList();

        return new HealthCheckResult(
            unhealthyTenants.Count == 0 ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            string.Join(", ", unhealthyTenants));
    }
}
