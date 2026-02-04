using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class MediaStorageQuotaActionFilter : IAsyncAuthorizationFilter, IOrderedFilter
{
    // Should be above the InternalMediaSizeFilter (900) to override its value.
    public int Order { get; } = new MediaSizeLimitAttribute().Order + 1;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var maxFileSize = await context
            .HttpContext
            .RequestServices
            .GetRequiredService<IMediaStorageQuotaService>()
            .GetRemainingMediaStorageQuotaBytesAsync();

        // Code below is copied from OrchardCore.Media.Services.MediaSizeLimitAttribute.InternalMediaSizeFilter with
        // only the necessary changes to satisfy the static code analysis checks. The purpose of this is to invoke the
        // filter with the remaining quota instead of MediaOptions.MaxFileSize. Make sure to keep this in sync.
        var effectiveFormPolicy = context.FindEffectivePolicy<IRequestFormLimitsPolicy>();
        if (effectiveFormPolicy == null || effectiveFormPolicy == this)
        {
            var features = context.HttpContext.Features;
            var formFeature = features.Get<IFormFeature>();

            if (formFeature?.Form == null)
            {
                // Request form has not been read yet, so set the limits
                var formOptions = new FormOptions
                {
                    MultipartBodyLengthLimit = maxFileSize,
                };

                features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));
            }
        }

        var effectiveRequestSizePolicy = context.FindEffectivePolicy<IRequestSizePolicy>();
        if (effectiveRequestSizePolicy == null)
        {
            // Will only be available when running OutOfProcess with Kestrel.
            var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

            if (maxRequestBodySizeFeature is { IsReadOnly: false })
            {
                maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
            }
        }
    }
}
