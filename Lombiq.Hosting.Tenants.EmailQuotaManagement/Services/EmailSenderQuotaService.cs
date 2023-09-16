using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailSenderQuotaService : ISmtpService
{
    private readonly IStringLocalizer<EmailSenderQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;
    private readonly IEmailQuotaEmailService _emailQuotaEmailService;
    private readonly IClock _clock;

    public EmailSenderQuotaService(
        ISmtpService smtpService,
        IStringLocalizer<EmailSenderQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IEmailQuotaEmailService emailQuotaEmailService,
        IClock clock)
    {
        _smtpService = smtpService;
        T = stringLocalizer;
        _quotaService = quotaService;
        _emailQuotaEmailService = emailQuotaEmailService;
        _clock = clock;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        if (!_quotaService.ShouldLimitEmails())
        {
            return await _smtpService.SendAsync(message);
        }

        var isQuotaOverResult = await _quotaService.IsQuotaOverTheLimitAsync();
        if (isQuotaOverResult.IsOverQuota)
        {
            await SendAlertEmailIfNecessary(isQuotaOverResult.EmailQuota);

            return SmtpResult.Failed(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _quotaService.IncreaseQuota(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }

    private async Task SendAlertEmailIfNecessary(EmailQuota emailQuota)
    {
        if (IsSameMonth(_clock.UtcNow, emailQuota.LastReminder)) return;

        var emailMessage = await _emailQuotaEmailService.CreateEmailForExceedingQuota();
        var reminderResult = await _smtpService.SendAsync(emailMessage);

        if (reminderResult == SmtpResult.Success)
        {
            emailQuota.LastReminder = _clock.UtcNow;
            _quotaService.SaveQuota(emailQuota);
        }
    }

    private static bool IsSameMonth(DateTime date1, DateTime date2) =>
        date1.Month == date2.Month && date1.Year == date2.Year;
}
