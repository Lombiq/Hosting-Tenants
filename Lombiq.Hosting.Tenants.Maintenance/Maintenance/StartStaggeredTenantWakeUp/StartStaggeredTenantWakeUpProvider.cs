using Lombiq.Hosting.Tenants.Maintenance.Helpers;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.StartStaggeredTenantWakeUp;

public class StartStaggeredTenantWakeUpProvider : MaintenanceProviderBase
{
    private readonly IStaggeredTenantWakeUpService _staggeredTenantWakeUpService;

    public StartStaggeredTenantWakeUpProvider(IStaggeredTenantWakeUpService staggeredTenantWakeUpService) =>
        _staggeredTenantWakeUpService = staggeredTenantWakeUpService;

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) => Task.FromResult(true);

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var staggeredTenantWakeUp = await _staggeredTenantWakeUpService.GetOrCreateStaggeredTenantWakeUpAsync();
        var part = staggeredTenantWakeUp.As<StaggeredTenantWakeUpPart>();

        // If there were no deployment since the latest run and the task is not finished, continue.
        // Else if the build version changes, start a new staggered tenant wake-up, because a new deployment happened.
        if (context.LatestExecution.BuildVersion == context.CurrentExecution.BuildVersion &&
            !part.IsFinished())
        {
            await StaggeredTenantWakeUpHelper.ExecuteStaggeredTenantWakeUpAsync(nameof(StartStaggeredTenantWakeUpProvider));
        }
        else if (context.LatestExecution.BuildVersion != context.CurrentExecution.BuildVersion)
        {
            await StaggeredTenantWakeUpHelper.ExecuteStaggeredTenantWakeUpAsync(
                nameof(StartStaggeredTenantWakeUpProvider),
                newVersion: true);
        }
    }
}
