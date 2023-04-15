using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class DynamicMediaSizeActionFilter : IAsyncAuthorizationFilter, IOrderedFilter
{
    public int Order => 950; // Set the order above the InternalMediaSizeFilter (900)

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Read the max file size from the configuration
        var maxFileSize = await context
            .HttpContext
            .RequestServices
            .GetRequiredService<IMediaQuoteService>()
            .GetRemainingMediaSpaceLeft();
        var formOptions = new FormOptions
        {
            MultipartBodyLengthLimit = maxFileSize,
        };

        context.HttpContext.Features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));

        var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
        }
    }
}
