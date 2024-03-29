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

    public async Task<long> GetRemainingMediaStorageQuotaBytesAsync()
    {
        var directoryContent = _mediaFileStore.GetDirectoryContentAsync(includeSubDirectories: true);

        var listed = await directoryContent.ToListAsync();
        var sumBytes = listed.Where(item => item.Length > 0).Sum(item => item.Length);
        var remainingStorageQuotaBytes = GetMaxStorageQuotaBytes() - sumBytes;

        return remainingStorageQuotaBytes < 0 ? 0 : remainingStorageQuotaBytes;
    }

    public long GetMaxStorageQuotaBytes() => _mediaStorageManagementOptions.MaximumStorageQuotaBytes;
}
