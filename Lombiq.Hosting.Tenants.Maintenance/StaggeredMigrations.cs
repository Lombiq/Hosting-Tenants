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
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(StaggeredTenantWakeUpPart), part => part
            .WithField<NumericField>(nameof(StaggeredTenantWakeUpPart.BatchSize), field => field
                .WithDisplayName("Batch Size")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The number of tenants to be processed in each step or in case of parallel processing the" +
                           " number of tenants started at once. If StaggeredTenantWakeUpOptions is set, these values will be ignored.",
                }))
            .WithField<TimeField>(nameof(StaggeredTenantWakeUpPart.BatchInterval), field => field
                .WithDisplayName("Time Span Between Batches")
                .WithSettings(new TimeFieldSettings
                {
                    // 1 second.
                    Step = "1",
                    Hint = "The time span between batches of tenants to be processed in hh:mm:ss format.",
                }))
            .WithField<BooleanField>(nameof(StaggeredTenantWakeUpPart.RunParallel), field => field
                .WithDisplayName("Run In Parallel")
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered tenant wake-up process should run in parallel or not.",
                }))
            .WithField<NumericField>(nameof(StaggeredTenantWakeUpPart.CurrentVersion), field => field
                .WithDisplayName("Current Version")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "The current version of the staggered tenant wake-up process.",
                }))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaggeredTenantWakeUp, builder => builder
            .WithPart<StaggeredTenantWakeUpPart>()
        );

        return 1;
    }
}
