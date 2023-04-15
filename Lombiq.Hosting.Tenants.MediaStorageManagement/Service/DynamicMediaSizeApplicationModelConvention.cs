using Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Media.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using System.Linq;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class DynamicMediaSizeApplicationModelConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        // Find the desired controller by name
        var targetController = application.Controllers.FirstOrDefault(controller =>
            controller.ControllerName == typeof(AdminController).ControllerName() &&
            controller.Actions.Any(action => action.ActionName == nameof(AdminController.Upload)));

        // Find the desired action by name
        var targetAction = targetController?.Actions.FirstOrDefault(action =>
            action.ActionName == nameof(AdminController.Upload));

        // Add the DynamicMediaSizeActionFilter to the action
        targetAction?.Filters.Add(new DynamicMediaSizeActionFilter());
    }
}
