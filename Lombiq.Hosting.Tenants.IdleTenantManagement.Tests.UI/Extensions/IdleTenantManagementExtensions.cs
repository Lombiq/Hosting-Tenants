using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
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
                        "1")
                    .AddWithValue(
                        "Logging:LogLevel:Lombiq.Hosting.Tenants.IdleTenantManagement.Services.IdleShutdownService",
                        "Information")
                    .AddWithValue(
                        "OrchardCore:OrchardCore_BackgroundService:PollingTime",
                        "00:00:05")
                    .AddWithValue(
                        "OrchardCore:OrchardCore_BackgroundService:MinimumIdleTime",
                        "00:00:01");

                return Task.CompletedTask;
            };

        configuration.OrchardCoreConfiguration.AfterFakeLoggingConfiguration =
            (_, fakeLogCollectorOptions) => fakeLogCollectorOptions.FilteredLevels.Add(LogLevel.Information);

        configuration.AssertAppLogsAsync = app =>
            app.LogsShouldNotContainAsync(
                logEntry => AppLogAssertionHelper.NotMediaCacheEntries(logEntry) && logEntry.Level != LogLevel.Information,
                cancellationToken: configuration.TestCancellationToken);
    }
}
