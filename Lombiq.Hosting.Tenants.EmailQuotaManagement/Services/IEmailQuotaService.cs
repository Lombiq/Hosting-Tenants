using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

/// <summary>
/// A service that is responsible for managing the quota of sent emails.
/// </summary>
public interface IEmailQuotaService
{
    Task<IEnumerable<string>> CollectUserEmailsForExceedingQuotaAsync();

    /// <summary>
    /// Checks if the emails should be limited.
    /// </summary>
    bool ShouldLimitEmails();

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

    /// <summary>
    /// Saves the given quota on reminder sent.
    /// </summary>
    void SaveQuotaReminder(EmailQuota emailQuota);

    /// <summary>
    /// Returns <see langword="true"/> if the reminder email should be sent.
    /// </summary>
    bool ShouldSendReminderEmail(EmailQuota emailQuota, int? currentPercentage = null);

    /// <summary>
    /// Returns the current quota usage percentage.
    /// </summary>
    int CurrentUsagePercentage(EmailQuota emailQuota);
}
