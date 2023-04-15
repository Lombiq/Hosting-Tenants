using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class UploadFileSizeFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;

    public UploadFileSizeFilter(IShapeFactory shapeFactory, ILayoutAccessor layoutAccessor)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.IsAdmin())
        {
            await next();
            return;
        }

        var layout = await _layoutAccessor.GetLayoutAsync();
        var contentZone = layout.Zones["Footer"];
        await contentZone.AddAsync(await _shapeFactory.CreateAsync("UploadFileSize"), "15");

        await next();
    }
}
