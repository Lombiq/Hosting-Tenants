using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.Events;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Handlers;

public class MediaStorageQuotaHandler : IMediaEventHandler
{
    private readonly IHttpContextAccessor _hca;

    public MediaStorageQuotaHandler(IHttpContextAccessor hca) =>
        _hca = hca;

    public async Task MediaPermittedStorageAsync(MediaPermittedStorageContext context)
    {
        if (_hca.HttpContext?.RequestServices.GetService<IMediaStorageQuotaService>() is { } mediaStorageQuotaService)
        {
            context.Constrain(await mediaStorageQuotaService.GetRemainingMediaStorageQuotaBytesAsync());
        }
    }
}
