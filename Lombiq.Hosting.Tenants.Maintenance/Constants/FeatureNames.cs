namespace Lombiq.Hosting.Tenants.Maintenance.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting.Tenants.Maintenance";
    public const string Maintenance = Module;
    public const string TenantUrlMaintenanceCore = Maintenance + "." + nameof(TenantUrlMaintenanceCore);
    public const string UpdateSiteUrl = Maintenance + "." + nameof(UpdateSiteUrl);
    public const string UpdateShellRequestUrls = Maintenance + "." + nameof(UpdateShellRequestUrls);
}
