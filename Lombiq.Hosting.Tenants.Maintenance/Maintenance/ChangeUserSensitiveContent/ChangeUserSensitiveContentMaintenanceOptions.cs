namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

public class ChangeUserSensitiveContentMaintenanceOptions
{
    public bool IsEnabled { get; set; }
    public string TenantNames { get; set; }
    public string EmailExcludePattern { get; set; } = ".+@lombiq.com$";
}
