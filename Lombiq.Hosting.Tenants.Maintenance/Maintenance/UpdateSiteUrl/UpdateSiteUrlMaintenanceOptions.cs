namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateSiteUrl;

public class UpdateSiteUrlMaintenanceOptions
{
    public bool IsEnabled { get; set; }
    public string DefaultTenantSiteUrl { get; set; }
    public string SiteUrl { get; set; }
}
