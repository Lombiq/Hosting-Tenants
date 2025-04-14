namespace Lombiq.Hosting.Tenants.Maintenance.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting.Tenants.Maintenance";
    public const string Maintenance = Module;
    public const string UpdateSiteUrl = Maintenance + "." + nameof(UpdateSiteUrl);
    public const string UpdateShellRequestUrls = Maintenance + "." + nameof(UpdateShellRequestUrls);
    public const string AddAdministratorRoleToUsersWithRole = Maintenance + "." + nameof(AddAdministratorRoleToUsersWithRole);
    public const string RemoveUsers = Maintenance + "." + nameof(RemoveUsers);
    public const string ChangeUserSensitiveContent = Maintenance + "." + nameof(ChangeUserSensitiveContent);
    public const string DeleteOrRebuildElasticsearchIndices = Maintenance + "." + nameof(DeleteOrRebuildElasticsearchIndices);
    public const string DeleteElasticsearchIndicesBeforeSetup = Maintenance + "." + nameof(DeleteElasticsearchIndicesBeforeSetup);
    public const string UpdateEnabledFeatures = Maintenance + "." + nameof(UpdateEnabledFeatures);
    public const string CustomMaintenance = Maintenance + "." + nameof(CustomMaintenance);
    public const string StaggeredMaintenance = Maintenance + "." + nameof(StaggeredMaintenance);
}
