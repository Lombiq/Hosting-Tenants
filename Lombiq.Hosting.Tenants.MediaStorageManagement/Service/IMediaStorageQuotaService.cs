using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

/// <summary>
/// Storage quota related calculations.
/// </summary>
public interface IMediaStorageQuotaService
{
    /// <summary>
    /// Returns the remaining storage space left from the quota in bytes. It is always a non-negative number, meaning
    /// the minimum value is 0.
    /// </summary>
    Task<long> GetRemainingMediaStorageQuotaBytesAsync();

    /// <summary>
    /// Returns the maximum storage space form the quota in bytes.
    /// </summary>
    long GetMaxStorageQuotaBytes();
}

public static class MediaStorageQuotaServiceExtensions
{
    /// <summary>
    /// Returns the maximum storage quota space in Megabytes.
    /// </summary>
    public static float GetMaxStorageQuotaMegabytes(this IMediaStorageQuotaService mediaStorageQuotaService) =>
        mediaStorageQuotaService.GetMaxStorageQuotaBytes() / 1024f / 1024f;
}
