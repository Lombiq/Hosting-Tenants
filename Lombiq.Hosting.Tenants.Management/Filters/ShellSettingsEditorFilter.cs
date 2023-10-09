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
            var tenantName = context.RouteData.Values["Id"].ToString();
            if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
            {
                await next();
                return;
            }

            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];
            var tenantSettingsPrefix = $"{tenantName}Prefix";
            var editableItems = shellSettings.ShellConfiguration.AsJsonNode();

            await contentZone.AddAsync(
                await _shapeFactory.CreateAsync<ShellSettingsEditorViewModel>(
                    "ShellSettingsEditor",
                    viewModel =>
                    {
                        viewModel.Json = editableItems[tenantSettingsPrefix]?.ToJsonString();
                        viewModel.TenantId = tenantName;
                    }),
                "10");
        }

        await next();
    }
}
