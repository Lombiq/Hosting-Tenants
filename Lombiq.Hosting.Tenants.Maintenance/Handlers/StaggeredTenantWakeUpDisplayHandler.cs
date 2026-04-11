using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Handlers;

public class StaggeredTenantWakeUpDisplayHandler : IContentDisplayHandler
{
    private readonly IStringLocalizer<StaggeredTenantWakeUpDisplayHandler> T;

    public StaggeredTenantWakeUpDisplayHandler(IStringLocalizer<StaggeredTenantWakeUpDisplayHandler> localizer) => T = localizer;

    public Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context) => Task.CompletedTask;

    public Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context) => Task.CompletedTask;

    public Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
    {
        if (contentItem.ContentType != ContentTypes.StaggeredTenantWakeUp ||
            !contentItem.TryGet<StaggeredTenantWakeUpPart>(out var staggeredTenantWakeUpPart))
        {
            return Task.CompletedTask;
        }

        if (staggeredTenantWakeUpPart.IsRunning())
        {
            context.Updater.ModelState.AddModelError(
                nameof(StaggeredTenantWakeUpPart.IsRunning),
                T["You can't save this while a staggered tenant wake-up is running."]);
        }

        return Task.CompletedTask;
    }
}
