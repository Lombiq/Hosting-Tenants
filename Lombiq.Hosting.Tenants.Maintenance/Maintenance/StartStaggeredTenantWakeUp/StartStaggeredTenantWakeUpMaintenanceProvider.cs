using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.StartStaggeredTenantWakeUp;

public class StartStaggeredTenantWakeUpMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IStaggeredTenantWakeUpService _staggeredTenantWakeUpService;

    private ContentItem _staggeredTenantWakeUp;

    public StartStaggeredTenantWakeUpMaintenanceProvider(IStaggeredTenantWakeUpService staggeredTenantWakeUpService) =>
        _staggeredTenantWakeUpService = staggeredTenantWakeUpService;

    public override async Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        _staggeredTenantWakeUp = await _staggeredTenantWakeUpService.GetOrCreateStaggeredTenantWakeUpSettingsAsync();
        return _staggeredTenantWakeUp.GetOrCreate<StaggeredTenantWakeUpPart>().RunOnStartup.Value;
    }

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var part = _staggeredTenantWakeUp.GetOrCreate<StaggeredTenantWakeUpPart>();

        // If there were no deployment since the latest run and the task is not finished, continue.
        // Else if the build version changes, start a new staggered tenant wake-up, because a new deployment happened.
        if (context.LatestExecution?.BuildVersion == context.CurrentExecution.BuildVersion &&
            !part.IsFinished())
        {
            await StaggeredTenantWakeUpHelper.ExecuteStaggeredTenantWakeUpAsync(nameof(StartStaggeredTenantWakeUpMaintenanceProvider));
        }
        else if (context.LatestExecution?.BuildVersion != context.CurrentExecution.BuildVersion)
        {
            await StaggeredTenantWakeUpHelper.ExecuteStaggeredTenantWakeUpAsync(
                nameof(StartStaggeredTenantWakeUpMaintenanceProvider),
                newVersion: true);
        }
    }
}
