using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using System;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.Maintenance.Indexes;

public class MaintenanceTaskExecutionIndex : MapIndex
{
    public string MaintenanceId { get; set; }
    public DateTime ExecutionTimeUtc { get; set; }
    public bool IsSuccess { get; set; }
}

public class MaintenanceTaskExecutionIndexProvider : IndexProvider<MaintenanceTaskExecutionData>
{
    public MaintenanceTaskExecutionIndexProvider() =>
        CollectionName = DocumentCollections.Maintenance;

    public override void Describe(DescribeContext<MaintenanceTaskExecutionData> context) =>
        context.For<MaintenanceTaskExecutionIndex>()
            .Map(execution => new MaintenanceTaskExecutionIndex
            {
                MaintenanceId = execution.MaintenanceId,
                ExecutionTimeUtc = execution.ExecutionTimeUtc,
                IsSuccess = execution.IsSuccess,
            });
}
