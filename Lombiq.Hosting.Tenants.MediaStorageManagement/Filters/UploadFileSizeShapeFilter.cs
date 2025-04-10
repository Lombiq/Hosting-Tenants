using Lombiq.HelpfulLibraries.OrchardCore.Contents;
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

public sealed class UploadFileSizeShapeFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IMediaStorageQuotaService _mediaStorageQuotaService;

    public UploadFileSizeShapeFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IMediaStorageQuotaService mediaStorageQuotaService)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _mediaStorageQuotaService = mediaStorageQuotaService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.IsAdmin())
        {
            await next();
            return;
        }

        if (context.Result is ViewResult &&
            context.IsMvcRoute(
                nameof(AdminController.Index),
                typeof(AdminController).ControllerName(),
                $"{nameof(OrchardCore)}.{nameof(OrchardCore.Media)}"))
        {
            await _layoutAccessor.AddShapeToZoneAsync(
                "Footer",
                await _shapeFactory.CreateAsync<UploadFileSizeViewModel>(
                    "UploadFileSize",
                    viewModel => viewModel.MaximumStorageQuotaMegabytes = _mediaStorageQuotaService.GetMaxStorageQuotaMegabytes()),
                "0");
        }

        await next();
    }
}
