using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

/// <summary>
/// Storage quota related calculations.
/// </summary>
public interface IMediaStorageQuotaService
{
    /// <summary>
    /// Returns remaining storage quota space left in bytes. It is always a non-negative number, meaning the minimum
    /// value is 0.
    /// </summary>
    Task<long> GetRemainingMediaStorageQuotaLeftAsync();

    /// <summary>
    /// Returns the maximum storage quota space in bytes.
    /// </summary>
    long MaxStorageQuotaForTenantInBytes();
}

public static class MediaStorageQuotaServiceExtensions
{
    /// <summary>
    /// Returns the maximum storage quota space in Megabytes.
    /// </summary>
    public static float MaxStorageQuotaForTenantInMegabytes(this IMediaStorageQuotaService mediaStorageQuotaService) =>
        mediaStorageQuotaService.MaxStorageQuotaForTenantInBytes() / 1024f / 1024f;
}
