using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using OrchardCore.Media.Events;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Handlers;

public class MediaStorageQuotaHandler : IMediaEventHandler
{
    private readonly IMediaStorageQuotaService _mediaStorageQuotaService;

    public MediaStorageQuotaHandler(IMediaStorageQuotaService mediaStorageQuotaService) =>
        _mediaStorageQuotaService = mediaStorageQuotaService;

    public async Task MediaPermittedStorageAsync(MediaPermittedStorageContext context) =>
        context.Constrain(await _mediaStorageQuotaService.GetRemainingMediaStorageQuotaBytesAsync());
}
