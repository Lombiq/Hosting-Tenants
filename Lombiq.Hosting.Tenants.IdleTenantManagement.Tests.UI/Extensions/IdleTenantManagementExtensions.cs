using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Tests.UI.Extensions;

public static class IdleTenantManagementExtensions
{
    public static void SetMaxIdleMinutesAndLoggingForUITest(
        this OrchardCoreUITestExecutorConfiguration configuration) => configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .Add("--OrchardCore:Lombiq_Hosting_Tenants_IdleTenantManagement:IdleShutdownOptions:MaxIdleMinutes")
                    .Add("1");

                argumentsBuilder
                    .Add("--Logging:LogLevel:Default")
                    .Add("Information");

                return Task.CompletedTask;
            };

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithIdleCheckAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain("Shutting down tenant \"Default\" because of idle timeout.");
}
