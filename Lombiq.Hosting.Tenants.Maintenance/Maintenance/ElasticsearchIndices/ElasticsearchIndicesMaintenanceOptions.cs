namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ElasticsearchIndices;

public class ElasticsearchIndicesMaintenanceOptions
{
    public bool DeleteMaintenanceIsEnabled { get; set; }
    public bool RebuildMaintenanceIsEnabled { get; set; }
    public bool BeforeSetupMiddlewareIsEnabled { get; set; }
}
