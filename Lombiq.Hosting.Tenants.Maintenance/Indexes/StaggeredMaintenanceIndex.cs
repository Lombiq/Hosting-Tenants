using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Text.Json;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.Maintenance.Indexes;

public class StaggeredMaintenanceIndex : MapIndex
{
    public decimal ProgressPercentage { get; set; }
    public decimal AllTenantCount { get; set; }
    public decimal ProcessedTenantsCount { get; set; }
    public decimal CurrentVersion { get; set; }
    public string ProcessedTenantNames { get; set; }
    public string ErrorLogs { get; set; }
}

public class StaggeredMaintenanceIndexProvider : IndexProvider<StaggeredMaintenancePart>
{
    public StaggeredMaintenanceIndexProvider() =>
        CollectionName = DocumentCollections.Maintenance;

    public override void Describe(DescribeContext<StaggeredMaintenancePart> context) =>
        context.For<StaggeredMaintenanceIndex>()
            .Map(part => new StaggeredMaintenanceIndex
            {
                ProgressPercentage = part.ProgressPercentage.Value ?? 0,
                AllTenantCount = part.AllTenantCount.Value ?? 0,
                ProcessedTenantsCount = part.ProcessedTenantsCount.Value ?? 0,
                CurrentVersion = part.CurrentVersion.Value ?? 0,
                ProcessedTenantNames = JsonSerializer.Serialize(part.ProcessedTenantIds),
                ErrorLogs = JsonSerializer.Serialize(part.ErrorLogs),
            });
}
