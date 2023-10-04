using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

/// <summary>
/// A service that is responsible for managing the quota of sent emails.
/// </summary>
public interface IEmailQuotaService
{
    /// <summary>
    /// Checks if the emails should be limited, depending on the default SMTP settings.
    /// </summary>
    bool ShouldLimitEmails();

    /// <summary>
    /// Checks if the current email usage is over monthly limit, returns a <see cref="QuotaResult"/> object that
    /// contains the <see cref="EmailQuota"/> and <see langword="true"/> in the <see cref="QuotaResult.IsOverQuota"/> if
    /// it is over the limit.
    /// </summary>
    Task<QuotaResult> IsQuotaOverTheLimitAsync();

    /// <summary>
    /// Gets or creates the <see cref="EmailQuota"/> in the database.
    /// </summary>
    Task<EmailQuota> GetOrCreateCurrentQuotaAsync();

    /// <summary>
    /// Increases the usage count of the given <paramref name="emailQuota"/> and saves it to the database.
    /// </summary>
    void IncreaseEmailUsage(EmailQuota emailQuota);

    /// <summary>
    /// Resets the given <paramref name="emailQuota"/> <see cref="EmailQuota.CurrentEmailUsageCount"/> to 0 and saves it
    /// to the database.
    /// </summary>
    void ResetQuota(EmailQuota emailQuota);

    /// <summary>
    /// Sets the <see cref="EmailQuota.LastReminderUtc"/> to the current date and
    /// <see cref="EmailQuota.LastReminderPercentage"/> to the current email quota usage percentage then saves it to the
    /// database.
    /// </summary>
    void SetQuotaOnEmailReminder(EmailQuota emailQuota);

    /// <summary>
    /// Returns <see langword="true"/>, if the reminder email should be sent.
    /// </summary>
    bool ShouldSendReminderEmail(EmailQuota emailQuota, int currentUsagePercentage);

    /// <summary>
    /// Return the email quota per month value from the configuration.
    /// </summary>
    int GetEmailQuotaPerMonth();
}
