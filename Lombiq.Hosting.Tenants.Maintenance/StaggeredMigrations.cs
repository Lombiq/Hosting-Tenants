using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class StaggeredMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public StaggeredMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;
    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(StaggeredMaintenancePart), part => part
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProgressPercentage), field => field
                .WithDisplayName("Progress Percentage")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "Calculated from proccessed tenants count and all tenant count.",
                }))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.AllTenantCount), field => field
                .WithDisplayName("All Tenant Count"))
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants to be processed. This is the total number of running tenants in the system.",
                })
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProcessedTenantsCount), field => field
                .WithDisplayName("Processed Tenant Count"))
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants already processed.",
                })
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProcessingStep), field => field
                .WithDisplayName("Processing Step"))
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants to be processed in each step. After a batch of tenants is processed," +
                           " the system will wait for the specified time span before processing the next batch.",
                })
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.CurrentVersion), field => field
                .WithDisplayName("Current Version"))
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The current version of the staggered maintenance process.",
                })
            .WithField<TimeField>(nameof(StaggeredMaintenancePart.TimeSpanBetweenBatches), field => field
                .WithDisplayName("Time Span Between Batches")
                .WithSettings(new TimeFieldSettings
                {
                    // 1 second.
                    Step = "1",
                    Hint = "The time span between batches of tenants to be processed in hh:mm:ss format. " +
                           "If StaggeredMaintenanceOptions is set, this value will be ignored.",
                }))
            .WithField<BooleanField>(nameof(StaggeredMaintenancePart.Canceled), field => field
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered maintenance process has been canceled manually.",
                }))
            .WithField<BooleanField>(nameof(StaggeredMaintenancePart.Running), field => field
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered maintenance process is currently running.",
                }))
            .WithField<DateTimeField>(nameof(StaggeredMaintenancePart.Started), field => field
                .WithSettings(new DateTimeFieldSettings
                {
                    Hint = "The date and time when the current version staggered maintenance process started.",
                }))
            .WithField<DateTimeField>(nameof(StaggeredMaintenancePart.Finished), field => field
                .WithSettings(new DateTimeFieldSettings
                {
                    Hint = "The date and time when the current version staggered maintenance process was finished or cancelled.",
                }))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaggeredMaintenance, builder => builder
            .WithPart<StaggeredMaintenancePart>()
        );
        return 1;
    }
}
