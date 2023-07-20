using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class MediaStorageQuotaActionFilter : IAsyncAuthorizationFilter, IOrderedFilter
{
    public int Order => 950; // Should be above the InternalMediaSizeFilter (900) to override its value.

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var maxFileSize = await context
            .HttpContext
            .RequestServices
            .GetRequiredService<IMediaStorageQuotaService>()
            .GetRemainingMediaStorageQuotaLeftAsync();

        var formOptions = new FormOptions
        {
            MultipartBodyLengthLimit = maxFileSize,
        };

        context.HttpContext.Features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));

        var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature is { IsReadOnly: false })
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
        }
    }
}
