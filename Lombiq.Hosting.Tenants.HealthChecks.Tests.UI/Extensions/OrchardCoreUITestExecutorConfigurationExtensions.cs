using Lombiq.Hosting.Tenants.HealthChecks.Constants;
using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Tests.UI.Extensions;

public static class OrchardCoreUITestExecutorConfigurationExtensions
{
    /// <summary>
    /// Configures UI tests for the "Lombiq Hosting - Tenants Health Checks" feature.
    /// </summary>
    public static void ConfigureTenantHealthCheckTests(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        configuration.ResponseLogFilters["Ignore expected non-ok responses from ~/health/live"] = args =>
            !args.Response.Url.Contains("/health/live");

        configuration.OrchardCoreConfiguration.BeforeAppStart += (_, argumentsBuilder) =>
        {
            argumentsBuilder.AddWithValue(ConfigurationKeys.IsEnabled, value: true);
            return Task.CompletedTask;
        };
    }
}
