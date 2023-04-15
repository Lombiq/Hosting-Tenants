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
        var targetController = application.Controllers.FirstOrDefault(controller =>
            controller.ControllerName == typeof(AdminController).ControllerName() &&
            controller.Actions.Any(action => action.ActionName == nameof(AdminController.Upload)));

        var targetAction = targetController?.Actions.FirstOrDefault(action =>
            action.ActionName == nameof(AdminController.Upload));

        targetAction?.Filters.Add(new DynamicMediaSizeActionFilter());
    }
}
