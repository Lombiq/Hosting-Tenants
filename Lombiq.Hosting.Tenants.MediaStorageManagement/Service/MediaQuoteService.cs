using Microsoft.Extensions.Options;
using OrchardCore.Media;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public class MediaQuoteService : IMediaQuoteService
{
    private readonly MediaOptions _mediaOptions;
    private readonly IMediaFileStore _mediaFileStore;

    public MediaQuoteService(IOptions<MediaOptions> mediaOptions, IMediaFileStore mediaFileStore)
    {
        _mediaOptions = mediaOptions.Value;
        _mediaFileStore = mediaFileStore;
    }

    public async Task<long> GetRemainingMediaSpaceLeft()
    {
        var directoryContent = _mediaFileStore.GetDirectoryContentAsync(includeSubDirectories: true);

        var listed = await directoryContent.ToListAsync();
        var sumSize = listed.Where(item => item.Length > 0).Sum(item => item.Length);
        return _mediaOptions.MaxFileSize - sumSize;
    }
}
