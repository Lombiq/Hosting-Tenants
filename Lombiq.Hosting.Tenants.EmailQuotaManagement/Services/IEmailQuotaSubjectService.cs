using Microsoft.Extensions.Localization;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

/// <summary>
/// Service for getting the subject of the email sent when the email quota is exceeded or when it's close to being exceeded.
/// </summary>
public interface IEmailQuotaSubjectService
{
    /// <summary>
    /// Gets the subject of the email sent for the email when the email usage is above 80%.
    /// </summary>
    /// <param name="percentage">The current usage percentage.</param>
    public LocalizedString GetWarningEmailSubject(int percentage);

    /// <summary>
    /// Gets the subject of the email sent for the email quota exceeded email.
    /// </summary>
    public LocalizedString GetExceededEmailSubject();
}
