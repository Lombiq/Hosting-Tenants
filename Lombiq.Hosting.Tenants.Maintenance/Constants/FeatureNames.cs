namespace Lombiq.Hosting.Tenants.Maintenance.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting.Tenants.Maintenance";
    public const string Maintenance = Module;
    public const string UpdateSiteUrl = Maintenance + "." + nameof(UpdateSiteUrl);
    public const string UpdateShellRequestUrls = Maintenance + "." + nameof(UpdateShellRequestUrls);
    public const string AddSiteOwnerPermissionToRole = Maintenance + "." + nameof(AddSiteOwnerPermissionToRole);
    public const string RemoveUsers = Maintenance + "." + nameof(RemoveUsers);
    public const string ChangeUserSensitiveContent = Maintenance + "." + nameof(ChangeUserSensitiveContent);
    public const string ResetStripeApiCredentials = Maintenance + "." + nameof(ResetStripeApiCredentials);
}
