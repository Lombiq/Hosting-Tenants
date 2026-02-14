using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;

public class MediaStorageQuotaActionFilter : IAsyncAuthorizationFilter, IOrderedFilter, IRequestFormLimitsPolicy, IRequestSizePolicy
{
    // Should be above the InternalMediaSizeFilter (900) to make it the effective policy, which will prevent
    // InternalMediaSizeFilter from doing anything.
    public int Order { get; } = new MediaSizeLimitAttribute().Order + 1;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var maxFileSize = await context
            .HttpContext
            .RequestServices
            .GetRequiredService<IMediaStorageQuotaService>()
            .GetRemainingMediaStorageQuotaBytesAsync();

        // This check is only necessary for non-multipart bodies. It becomes relevant when the quota is nearly exhausted
        // and even a small file (that doesn't require multipart) can't be uploaded. It also protects from maliciously
        // uploading a huge number of tiny files to overfill storage beyond the quota.
        if (context.HttpContext.Request.ContentLength > maxFileSize)
        {
            context.Result = new BadRequestResult();
            return;
        }

        var formOptions = new FormOptions
        {
            MultipartBodyLengthLimit = maxFileSize,
        };

        context.HttpContext.Features.Set<IFormFeature>(CreateFormFeatureWithNewOptions(context, formOptions));

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

    private static FormFeature CreateFormFeatureWithNewOptions(AuthorizationFilterContext context, FormOptions formOptions)
    {
        var form = context.HttpContext.Features.Get<IFormFeature>()?.Form;
        return new FormFeature(context.HttpContext.Request, formOptions)
        {
            Form = form,
        };
    }
}
