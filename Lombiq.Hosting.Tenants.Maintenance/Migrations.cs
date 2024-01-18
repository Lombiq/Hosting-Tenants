using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using OrchardCore.Data.Migration;
using System;
using System.Threading.Tasks;
using YesSql.Sql;

namespace Lombiq.Hosting.Tenants.Maintenance;

public class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<MaintenanceTaskExecutionIndex>(
            table => table
                .Column<string>(nameof(MaintenanceTaskExecutionIndex.MaintenanceId))
                .Column<DateTime>(nameof(MaintenanceTaskExecutionIndex.ExecutionTimeUtc))
                .Column<bool>(nameof(MaintenanceTaskExecutionIndex.IsSuccess)),
            collection: DocumentCollections.Maintenance);

        await SchemaBuilder.AlterIndexTableAsync<MaintenanceTaskExecutionIndex>(
            table => table
                .CreateIndex(
                    $"IDX_{nameof(MaintenanceTaskExecutionIndex)}_{nameof(MaintenanceTaskExecutionIndex.MaintenanceId)}",
                    nameof(MaintenanceTaskExecutionIndex.MaintenanceId)),
            collection: DocumentCollections.Maintenance);

        return 1;
    }
}
