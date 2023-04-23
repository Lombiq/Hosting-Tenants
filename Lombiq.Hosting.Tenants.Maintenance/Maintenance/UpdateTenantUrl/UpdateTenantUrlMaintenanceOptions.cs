namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateTenantUrl;

public class UpdateTenantUrlMaintenanceOptions
{
    public bool Enabled { get; set; }
    public string DefaultTenantUrl { get; set; }
    public string TenantUrl { get; set; }
}
