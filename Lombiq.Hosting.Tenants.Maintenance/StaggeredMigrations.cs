using Lombiq.HelpfulExtensions.Constants;
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
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.BatchSize), field => field
                .WithDisplayName("Batch Size")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants to be processed in each step or in case of parallel processing the" +
                           " number of tenants started at once.",
                }))
            .WithField<TimeField>(nameof(StaggeredMaintenancePart.BatchInterval), field => field
                .WithDisplayName("Time Span Between Batches")
                .WithSettings(new TimeFieldSettings
                {
                    // 1 second.
                    Step = "1",
                    Hint = "The time span between batches of tenants to be processed in hh:mm:ss format. " +
                           "If StaggeredMaintenanceOptions is set, this value will be ignored.",
                }))
            .WithField<BooleanField>(nameof(StaggeredMaintenancePart.RunParallel), field => field
                .WithDisplayName("Run In Parallel")
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered maintenance process should run in parallel or not.",
                }))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.CurrentVersion), field => field
                .WithDisplayName("Current Version")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The current version of the staggered maintenance process.",
                }))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.ProgressPercentage), field => field
                .WithDisplayName("Progress Percentage")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "Calculated from processed tenants count and all tenant count.",
                })
                .WithEditor(Editors.Label))
            .WithField<NumericField>(nameof(StaggeredMaintenancePart.AllTenantCount), field => field
                .WithDisplayName("All Tenant Count")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants to be processed. This is the total number of running tenants in the system.",
                })
                .WithEditor(Editors.Label))
            .WithField<BooleanField>(nameof(StaggeredMaintenancePart.Canceled), field => field
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered maintenance process has been canceled manually.",
                })
                .WithEditor(Editors.Label))
            .WithField<DateTimeField>(nameof(StaggeredMaintenancePart.Started), field => field
                .WithSettings(new DateTimeFieldSettings
                {
                    Hint = "The date and time when the current version staggered maintenance process started.",
                })
                .WithEditor(Editors.Label))
            .WithField<DateTimeField>(nameof(StaggeredMaintenancePart.Finished), field => field
                .WithSettings(new DateTimeFieldSettings
                {
                    Hint = "The date and time when the current version staggered maintenance process was finished or canceled.",
                })
                .WithEditor(Editors.Label))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaggeredMaintenance, builder => builder
            .WithPart<StaggeredMaintenancePart>()
        );

        return 1;
    }
}
