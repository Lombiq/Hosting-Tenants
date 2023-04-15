using Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class DynamicMediaSizeApplicationModelConvention : IApplicationModelConvention
{
    private readonly DynamicMediaSizeActionFilter _mediaSizeFilter;

    public DynamicMediaSizeApplicationModelConvention() =>
        _mediaSizeFilter = new DynamicMediaSizeActionFilter();

    public void Apply(ApplicationModel application)
    {
        // Find the desired controller by name
        var targetController = application.Controllers.FirstOrDefault(controller =>
            controller.ControllerName == "Admin" &&
            controller.Actions.Any(action => action.ActionName == "Upload"));

        if (targetController != null)
        {
            // Find the desired action by name
            var targetAction = targetController.Actions.FirstOrDefault(action => action.ActionName == "Upload");

            // Add the DynamicMediaSizeActionFilter to the action
            if (targetAction != null)
            {
                targetAction.Filters.Add(_mediaSizeFilter);
            }
        }
    }
}
