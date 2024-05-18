using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class MediaStorageQuotaService : IMediaStorageQuotaService
{
    private readonly MediaStorageManagementOptions _mediaStorageManagementOptions;

    public MediaStorageQuotaService(IOptions<MediaStorageManagementOptions> mediaStorageManagementOptions) =>
        _mediaStorageManagementOptions = mediaStorageManagementOptions.Value;

    public Task<long> GetRemainingMediaStorageQuotaBytesAsync() => Task.FromResult(1L);

    public long GetMaxStorageQuotaBytes() => _mediaStorageManagementOptions.MaximumStorageQuotaBytes;
}
