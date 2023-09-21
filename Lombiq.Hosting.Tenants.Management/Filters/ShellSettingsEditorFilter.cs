using Lombiq.Hosting.Tenants.Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Filters;

public class ShellSettingsEditorFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;
    private readonly IShellHost _shellHost;



    public ShellSettingsEditorFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory,
        IShellHost shellHost)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
        _shellHost = shellHost;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var actionRouteController = context.ActionDescriptor.RouteValues["Controller"];
        var actionRouteArea = context.ActionDescriptor.RouteValues["Area"];
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(AdminController).ControllerName() &&
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}" &&
            actionRouteValue is nameof(AdminController.Edit) &&
            context.Result is ViewResult)
        {
            if (!_shellHost.TryGetSettings(context.RouteData.Values["Id"].ToString(), out var shellSettings))
            {
                return;
            }

            if (shellSettings != null)
            {
                var layout = await _layoutAccessor.GetLayoutAsync();
                var contentZone = layout.Zones["Content"];
                var asJsonNode = shellSettings.ShellConfiguration.AsJsonNode();
                var jsonString = asJsonNode.ToJsonString();
                await contentZone.AddAsync(
                    await _shapeFactory.CreateAsync<ShellSettingsEditorViewModel>(
                        "ShellSettingsEditor",
                        viewModel =>
                        {
                            viewModel.Settings = jsonString;
                            viewModel.TenantId = context.RouteData.Values["Id"].ToString();
                        }),
                    "6");
            }
        }

        await next();
    }


}
