using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Tests.UI.Extensions;

public static class ConfigurationExtensions
{
    public static void SetEnvironmentRobotsOptionsConfiguration(
        this OrchardCoreUITestExecutorConfiguration configuration,
        bool isProduction) =>
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_EnvironmentRobots:EnvironmentRobotsOptions:IsProduction",
                        value: isProduction);

                return Task.CompletedTask;
            };
}
