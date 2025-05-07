using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class StaggeredTenantWakeUpMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public StaggeredTenantWakeUpMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(StaggeredTenantWakeUpPart), part => part
            .WithField<NumericField>(nameof(StaggeredTenantWakeUpPart.BatchSize), field => field
                .WithDisplayName("Batch Size")
                .WithSettings(new NumericFieldSettings
                {
                    Scale = 0,
                    Hint = "The number of tenants to be processed in each step or in case of parallel processing the" +
                        " number of tenants started at once.",
                }))
            .WithField<NumericField>(nameof(StaggeredTenantWakeUpPart.BatchIntervalSeconds), field => field
                .WithDisplayName("Batch Interval Second")
                .WithSettings(new NumericFieldSettings
                {
                    Scale = 0,
                    Hint = "The seconds between batches of tenants to be processed.",
                }))
            .WithField<BooleanField>(nameof(StaggeredTenantWakeUpPart.RunParallel), field => field
                .WithDisplayName("Run In Parallel")
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered tenant wake-up process should run in parallel or not.",
                }))
            .WithField<BooleanField>(nameof(StaggeredTenantWakeUpPart.RunOnStartup), field => field
                .WithDisplayName("Run On Startup")
                .WithSettings(new BooleanFieldSettings
                {
                    Hint = "Indicates whether the staggered tenant wake-up process should run on startup or not.",
                }))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaggeredTenantWakeUp, builder => builder
            .WithPart<StaggeredTenantWakeUpPart>()
        );

        return 1;
    }
}
