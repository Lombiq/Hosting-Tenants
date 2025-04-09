using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Indexes;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using System.Threading.Tasks;
using YesSql.Sql;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;
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

        await RunStaggeredMigrationsAsync();
        return 2;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await RunStaggeredMigrationsAsync();
        return 2;
    }

    private async Task RunStaggeredMigrationsAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(StaggeredMaintenancePart), part => part
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProgressPercentage))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.AllTenantCount))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.CurrentVersion))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProcessingStep))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProcessedTenantsCount)));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaggeredMaintenance, builder => builder
            .WithPart<StaggeredMaintenancePart>()
            .Listable());

        await SchemaBuilder.CreateMapIndexTableAsync<StaggeredMaintenanceIndex>(
            table => table
                .Column<string>(nameof(StaggeredMaintenanceIndex.ProgressPercentage))
                .Column<decimal>(nameof(StaggeredMaintenanceIndex.AllTenantCount))
                .Column<decimal>(nameof(StaggeredMaintenanceIndex.CurrentVersion))
                .Column<decimal>(nameof(StaggeredMaintenanceIndex.ProcessedTenantsCount)),
            collection: DocumentCollections.Maintenance);
    }
}
