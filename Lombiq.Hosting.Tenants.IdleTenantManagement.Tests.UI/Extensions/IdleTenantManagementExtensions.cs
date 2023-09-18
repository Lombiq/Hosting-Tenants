using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Shouldly;
using System;
using System.Threading.Tasks;
using static Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Constants.IdleTenantData;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Extensions;

public static class IdleTenantManagementExtensions
{
    public static void SetMaxIdleMinutesAndLoggingForUITest(
        this OrchardCoreUITestExecutorConfiguration configuration) => configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_IdleTenantManagement:IdleShutdownOptions:MaxIdleMinutes",
                        "1");

                argumentsBuilder
                    .AddWithValue("Logging:LogLevel:Default", "Information");

                return Task.CompletedTask;
            };

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithIdleCheckAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain($"Shutting down tenant \"{IdleTenantName}\" because of idle timeout.");
}
