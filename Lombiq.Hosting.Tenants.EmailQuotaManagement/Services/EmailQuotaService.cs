using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaService : ISmtpService
{
    private readonly IStringLocalizer<EmailQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly SmtpSettings _smtpOptions;
    private readonly IEmailQuotaEmailService _emailQuotaEmailService;
    private readonly IClock _clock;

    public EmailQuotaService(
        ISmtpService smtpService,
        IOptions<SmtpSettings> smtpOptions,
        IStringLocalizer<EmailQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IShellConfiguration shellConfiguration,
        IEmailQuotaEmailService emailQuotaEmailService,
        IClock clock)
    {
        _smtpService = smtpService;
        _smtpOptions = smtpOptions.Value;
        T = stringLocalizer;
        _quotaService = quotaService;
        _shellConfiguration = shellConfiguration;
        _emailQuotaEmailService = emailQuotaEmailService;
        _clock = clock;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var originalHost = _shellConfiguration.GetValue<string>("SmtpSettings:Host");
        if (originalHost != _smtpOptions.Host)
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
