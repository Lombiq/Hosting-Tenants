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
            .GetRemainingMediaStorageQuotaBytesAsync();

        var formOptions = new FormOptions
        {
            MultipartBodyLengthLimit = maxFileSize,
        };

        context.HttpContext.Features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));

        var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        // Only setting MaxRequestBodySize if it wouldn't go over the preconfigured size. This is necessary because
        // larger requests would pose a security issue (since the original limit was configured for a reason), and under
        // IIS it wouldn't work with the following message anyway: "Increasing the MaxRequestBodySize conflicts with the
        // max value for IIS limit maxAllowedContentLength. HTTP requests that have a content length greater than
        // maxAllowedContentLength will still be rejected by IIS. You can disable the limit by either removing or
        // setting the maxAllowedContentLength value to a higher limit."
        if (maxRequestBodySizeFeature is { IsReadOnly: false } && maxRequestBodySizeFeature.MaxRequestBodySize > maxFileSize)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
        }
    }
}
