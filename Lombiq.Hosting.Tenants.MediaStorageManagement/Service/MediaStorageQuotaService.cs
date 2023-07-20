using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class MediaStorageQuotaService : IMediaStorageQuotaService
{
    private readonly MediaStorageManagementOptions _mediaStorageManagementOptions;
    private readonly IMediaFileStore _mediaFileStore;

    public MediaStorageQuotaService(
        IOptions<MediaStorageManagementOptions> mediaStorageManagementOptions,
        IMediaFileStore mediaFileStore)
    {
        _mediaStorageManagementOptions = mediaStorageManagementOptions.Value;
        _mediaFileStore = mediaFileStore;
    }

    public async Task<long> GetRemainingMediaSpaceQuotaLeftAsync()
    {
        var directoryContent = _mediaFileStore.GetDirectoryContentAsync(includeSubDirectories: true);

        var listed = await directoryContent.ToListAsync();
        var sumSize = listed.Where(item => item.Length > 0).Sum(item => item.Length);
        var remainingSpace = MaxSpaceForTenantInBytes() - sumSize;

        return remainingSpace < 0 ? 0 : remainingSpace;
    }

    public long MaxSpaceForTenantInBytes() => _mediaStorageManagementOptions.MaximumStorageQuotaBytes;

    public float MaxSpaceForTenantInMegabytes() => MaxSpaceForTenantInBytes() / 1024f / 1024f;
}
