using Lombiq.Tests.UI.Services;
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

                return Task.CompletedTask;
            };

    public static void SetAddSiteOwnerPermissionToRoleMaintenanceConfiguration(
        this OrchardCoreUITestExecutorConfiguration configuration) => configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:AddSiteOwnerPermissionToRole:IsEnabled",
                        value: true)
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:AddSiteOwnerPermissionToRole:Role",
                        value: "Editor");

                return Task.CompletedTask;
            };

    public static void ChangeUserSensitiveContentMaintenanceConfiguration(
    this OrchardCoreUITestExecutorConfiguration configuration) => configuration.OrchardCoreConfiguration.BeforeAppStart +=
        (_, argumentsBuilder) =>
        {
            argumentsBuilder
                .AddWithValue(
                    "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:ChangeUserSensitiveContent:IsEnabled",
                    value: true)
                .AddWithValue(
                    "OrchardCore:Lombiq_Hosting_Tenants_Maintenance:ChangeUserSensitiveContent:TenantNames",
                    value: "Default");

            return Task.CompletedTask;
        };
}
