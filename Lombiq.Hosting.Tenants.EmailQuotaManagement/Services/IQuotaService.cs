using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

/// <summary>
/// A service that is responsible for managing the quota of sent emails.
/// </summary>
public interface IQuotaService
{
    /// <summary>
    /// Checks if the quota is over the limit.
    /// </summary>
    Task<QuotaResult> IsQuotaOverTheLimitAsync();

    /// <summary>
    /// Gets the current quota.
    /// </summary>
    Task<EmailQuota> GetCurrentQuotaAsync();

    /// <summary>
    /// Increases the given quota value.
    /// </summary>
    void IncreaseQuota(EmailQuota emailQuota);

    /// <summary>
    /// Resets the given quota to 0.
    /// </summary>
    void ResetQuota(EmailQuota emailQuota);
}
