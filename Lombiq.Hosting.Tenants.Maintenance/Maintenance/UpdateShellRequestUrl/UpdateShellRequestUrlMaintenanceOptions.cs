namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateShellRequestUrl;

public class UpdateShellRequestUrlMaintenanceOptions
{
    public bool IsEnabled { get; set; }
    public string DefaultShellRequestUrl { get; set; }
    public string RequestUrl { get; set; }
    public string RequestUrlPrefix { get; set; }
}
