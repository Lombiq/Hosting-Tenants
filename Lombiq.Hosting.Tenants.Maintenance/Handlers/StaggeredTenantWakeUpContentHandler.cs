using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Handlers;

public class StaggeredTenantWakeUpContentHandler : ContentHandlerBase
{
    private readonly StaggeredTenantWakeUpOptions _staggeredTenantWakeUpOptions;

    public StaggeredTenantWakeUpContentHandler(IOptions<StaggeredTenantWakeUpOptions> staggeredTenantWakeUpOptions) =>
        _staggeredTenantWakeUpOptions = staggeredTenantWakeUpOptions.Value;

    public override Task InitializingAsync(InitializingContentContext context)
    {
        var contentItem = context.ContentItem;
        if (contentItem.ContentType != Constants.ContentTypes.StaggeredTenantWakeUp ||
            contentItem.GetOrCreate<StaggeredTenantWakeUpPart>() is null ||
            !contentItem.IsNew())
        {
            return Task.CompletedTask;
        }

        contentItem.Alter<StaggeredTenantWakeUpPart>(part =>
        {
            part.BatchIntervalSeconds.Value = _staggeredTenantWakeUpOptions.BatchIntervalSeconds ?? part.BatchIntervalSeconds.Value;
            part.BatchSize.Value = _staggeredTenantWakeUpOptions.BatchSize ?? part.BatchSize.Value;
            part.RunParallel.Value = _staggeredTenantWakeUpOptions.RunParallel == true;
            part.RunOnStartup.Value = _staggeredTenantWakeUpOptions.RunOnStartup == true;
        });

        return Task.CompletedTask;
    }
}
