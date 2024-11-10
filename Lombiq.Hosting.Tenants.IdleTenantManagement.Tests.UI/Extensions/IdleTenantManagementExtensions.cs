using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Extensions;

public static class IdleTenantManagementExtensions
{
    public static void ConfigureIdleTenantManagementTestSettings(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_IdleTenantManagement:IdleShutdownOptions:MaxIdleMinutes",
                        "1");

                return Task.CompletedTask;
            };

        configuration.OrchardCoreConfiguration.AfterFakeLoggingConfiguration +=
            (_, fakeLogCollectorOptions) => fakeLogCollectorOptions.FilteredLevels.Add(LogLevel.Information);
    }
}
