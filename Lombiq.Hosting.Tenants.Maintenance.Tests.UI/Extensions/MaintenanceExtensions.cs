using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Extensions;

public static class MaintenanceExtensions
{
    public static void SetUpdateSiteUrlMaintenanceConfiguration(
        this OrchardCoreUITestExecutorConfiguration configuration) => configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:UpdateSiteUrl:IsEnabled",
                        value: true)
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:UpdateSiteUrl:DefaultTenantSiteUrl",
                        value: "https://test.com");

                argumentsBuilder
                    .AddWithValue("Logging:LogLevel:Default", "Debug");

                return Task.CompletedTask;
            };

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithMaintenanceExecutionStartAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain("Executing maintenance tasks on shell 'Default'.");

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithSuccessfulUpdateSiteUrlExecutionAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain("Maintenance task UpdateSiteUrlMaintenanceProvider executed successfully.");

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithSkippedUpdateShellRequestUrlExecutionAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain("Maintenance task UpdateShellRequestUrlsMaintenanceProvider is not needed.");
}
