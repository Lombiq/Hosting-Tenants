using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

/// <summary>
/// Storage quota related calculations.
/// </summary>
public interface IMediaStorageQuotaService
{
    /// <summary>
    /// Returns remaining quota space left in bytes. It is always a non-negative number, meaning the minimum value is 0.
    /// </summary>
    Task<long> GetRemainingMediaSpaceQuotaLeftAsync();

    /// <summary>
    /// Returns the maximum quota space in bytes.
    /// </summary>
    long MaxSpaceForTenantInBytes();

    /// <summary>
    /// Returns the maximum quota space in Megabytes.
    /// </summary>
    float MaxSpaceForTenantInMegabytes();
}
