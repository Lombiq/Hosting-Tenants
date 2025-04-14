using Lombiq.HelpfulLibraries.OrchardCore.Contents;
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

public sealed class ShellSettingsEditorFilter : IAsyncResultFilter
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
        if (context.IsNotFullViewRendering() ||
            !context.IsMvcRoute(
                nameof(AdminController.Edit),
                typeof(AdminController).ControllerName(),
                $"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}"))
        {
            await next();
            return;
        }

        var tenantName = context.RouteData.Values["Id"].ToString();
        if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            await next();
            return;
        }

        (context.Controller as Controller)!.TempData.TryGetValue("ValidationErrorJson", out var validationErrorJson);

        var editableItems = shellSettings.ShellConfiguration.AsJsonNode();
        var editorJson = string.IsNullOrEmpty(validationErrorJson?.ToString())
            ? editableItems[$"{tenantName}Prefix"]?.ToJsonString()
            : validationErrorJson.ToString();

        await _layoutAccessor.AddShapeToZoneAsync(
            "Content",
            await _shapeFactory.CreateAsync<ShellSettingsEditorViewModel>(
                "ShellSettingsEditor",
                viewModel =>
                {
                    viewModel.Json = editorJson;
                    viewModel.TenantId = tenantName;
                }),
            "10");

        await next();
    }
}
