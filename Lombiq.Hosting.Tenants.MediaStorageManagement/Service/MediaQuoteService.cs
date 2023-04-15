using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class MediaQuoteService : IMediaQuoteService
{
    private readonly MediaStorageManagementOptions _mediaStorageManagementOptions;
    private readonly IMediaFileStore _mediaFileStore;

    public MediaQuoteService(
        IOptions<MediaStorageManagementOptions> mediaStorageManagementOptions,
        IMediaFileStore mediaFileStore)
    {
        _mediaStorageManagementOptions = mediaStorageManagementOptions.Value;
        _mediaFileStore = mediaFileStore;
    }

    public async Task<long> GetRemainingMediaSpaceLeftAsync()
    {
        var directoryContent = _mediaFileStore.GetDirectoryContentAsync(includeSubDirectories: true);

        var listed = await directoryContent.ToListAsync();
        var sumSize = listed.Where(item => item.Length > 0).Sum(item => item.Length);
        return MaxSpaceForTenantInBytes() - sumSize;
    }

    public long MaxSpaceForTenantInBytes() => _mediaStorageManagementOptions.MaximumStorageQuote;
    public float MaxSpaceForTenantInMegabytes() => MaxSpaceForTenantInBytes() / 1024f / 1024f;
}
