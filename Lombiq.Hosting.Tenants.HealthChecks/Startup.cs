using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Management;

public sealed class TenantHealthChecksStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<HealthCheckOptions>(options => options.ResponseWriter = (context, report) =>
        {
            if (report.Status != HealthStatus.Healthy)
            {
//                context
            }

            return options.ResponseWriter(context, report);
        });
    }
}
