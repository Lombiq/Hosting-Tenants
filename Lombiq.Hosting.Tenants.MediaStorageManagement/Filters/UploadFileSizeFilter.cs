using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Lombiq.Hosting.Tenants.MediaStorageManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Media.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class UploadFileSizeFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IMediaQuoteService _mediaQuoteService;

    public UploadFileSizeFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IMediaQuoteService mediaQuoteService)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _mediaQuoteService = mediaQuoteService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.IsAdmin())
        {
            await next();
            return;
        }

        var actionRouteController = context.ActionDescriptor.RouteValues["Controller"];
        var actionRouteArea = context.ActionDescriptor.RouteValues["Area"];
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(AdminController).ControllerName() &&
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Media)}" &&
            actionRouteValue is nameof(AdminController.Index) &&
            context.Result is ViewResult)
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Footer"];
            var maximumSpace = _mediaQuoteService.MaxSpaceForTenantInMegabytes();
            await contentZone.AddAsync(await _shapeFactory.CreateAsync<UploadFileSizeViewModel>(
                "UploadFileSize",
                viewModel => viewModel.MaximumSpace = maximumSpace));
        }

        await next();
    }
}
