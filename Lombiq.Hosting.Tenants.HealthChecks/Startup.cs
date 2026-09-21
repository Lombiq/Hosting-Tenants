using Lombiq.Hosting.Tenants.Management.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Management;

public sealed class TenantHealthChecksStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Decorate<HealthCheckService, ReportingHealthCheckService>();
    }
}
