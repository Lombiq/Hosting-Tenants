using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.UI.Extensions;

public static class ConfigurationExtensions
{
    public static void SetUpdateSiteUrlConfiguration(
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
                    .AddWithValue("Logging:LogLevel:Default", "Information");

                return Task.CompletedTask;
            };

    public static readonly Func<IWebApplicationInstance, Task> AssertAppLogsWithIdleCheckAsync =
        async webApplicationInstance =>
            (await webApplicationInstance.GetLogOutputAsync())
            .ShouldContain("Shutting down tenant \"Default\" because of idle timeout.");
}
