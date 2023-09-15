using OrchardCore.Email;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

/// <summary>
/// This service is responsible for creating the email that will be sent to the site owners when the email quota is
/// exceeded.
/// </summary>
public interface IEmailQuotaEmailService
{
    /// <summary>
    /// Creates the <see cref="MailMessage"/> that could be sent to the site owners when the email quota is exceeded.
    /// </summary>
    Task<MailMessage> CreateEmailForExceedingQuota();
}
