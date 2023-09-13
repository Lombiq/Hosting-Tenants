using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaService : ISmtpService
{
    private readonly EmailQuotaOptions _emailQuotaOptions;
    private readonly IStringLocalizer<EmailQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;

    public EmailQuotaService(
        ISmtpService smtpService,
        IOptions<EmailQuotaOptions> emailQuotaOptions,
        IStringLocalizer<EmailQuotaService> stringLocalizer,
        IQuotaService quotaService)
    {
        _emailQuotaOptions = emailQuotaOptions.Value;
        T = stringLocalizer;
        _smtpService = smtpService;
        _quotaService = quotaService;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var currentQuota = await _quotaService.GetCurrentQuotaAsync();
        if (_emailQuotaOptions.EmailQuota < currentQuota.CurrentEmailQuotaCount)
        {
            return SmtpResult.Failed(T["The email quota ({currentQuota}) for the site has been exceeded.", _emailQuotaOptions.EmailQuota]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _quotaService.IncreaseQuota(currentQuota);
        }

        return emailResult;
    }
}
