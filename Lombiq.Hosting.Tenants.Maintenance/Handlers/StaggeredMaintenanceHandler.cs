using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Handlers;

public class StaggeredMaintenanceHandler : IContentDisplayHandler
{
    private readonly IStringLocalizer<StaggeredMaintenanceHandler> T;

    public StaggeredMaintenanceHandler(IStringLocalizer<StaggeredMaintenanceHandler> localizer) => T = localizer;

    public Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context) => Task.CompletedTask;

    public Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context) => Task.CompletedTask;

    public Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
    {
        if (contentItem.ContentType != ContentTypes.StaggeredMaintenance)
        {
            return Task.CompletedTask;
        }

        var staggeredMaintenancePart = contentItem.As<StaggeredMaintenancePart>();
        if (staggeredMaintenancePart == null) return Task.CompletedTask;

        if (staggeredMaintenancePart.IsRunning())
        {
            context.Updater.ModelState.AddModelError(
                nameof(StaggeredMaintenancePart.IsRunning),
                T["You can't save this while a staggered maintenance is running."]);
        }

        return Task.CompletedTask;
    }
}
