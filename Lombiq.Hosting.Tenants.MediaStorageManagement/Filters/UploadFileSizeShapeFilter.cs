using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Lombiq.Hosting.Tenants.MediaStorageManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class UploadFileSizeShapeFilter : IAsyncResultFilter, IAsyncActionFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IMediaStorageQuotaService _mediaStorageQuotaService;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    public UploadFileSizeShapeFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IMediaStorageQuotaService mediaStorageQuotaService,
        INotifier notifier,
        IHtmlLocalizer<UploadFileSizeShapeFilter> htmlLocalizer)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _mediaStorageQuotaService = mediaStorageQuotaService;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!AdminAttribute.IsApplied(context.HttpContext))
        {
            await next();
            return;
        }

        await _notifier.ErrorAsync(
            H["Due to scheduled maintenance, you can't upload files at the moment. Please try again in a few hours."]);

        await next();
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
            var maximumStorageQuotaMegabytes = _mediaStorageQuotaService.GetMaxStorageQuotaMegabytes();
            await contentZone.AddAsync(await _shapeFactory.CreateAsync<UploadFileSizeViewModel>(
                "UploadFileSize",
                viewModel => viewModel.MaximumStorageQuotaMegabytes = maximumStorageQuotaMegabytes));
        }

        await next();
    }
}
