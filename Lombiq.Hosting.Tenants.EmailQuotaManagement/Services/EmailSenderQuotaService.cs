using Lombiq.HelpfulExtensions.Extensions.Emails.Services;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
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
    private readonly ShellSettings _shellSettings;
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailSenderQuotaService(
        ISmtpService smtpService,
        IStringLocalizer<EmailSenderQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IEmailQuotaEmailService emailQuotaEmailService,
        IClock clock,
        ShellSettings shellSettings,
        IEmailTemplateService emailTemplateService)
    {
        _smtpService = smtpService;
        T = stringLocalizer;
        _quotaService = quotaService;
        _emailQuotaEmailService = emailQuotaEmailService;
        _clock = clock;
        _shellSettings = shellSettings;
        _emailTemplateService = emailTemplateService;
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
            await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota);

            return SmtpResult.Failed(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _quotaService.IncreaseQuota(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }

    private Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota)
    {
        if (IsSameMonth(_clock.UtcNow, emailQuota.LastReminder)) return Task.CompletedTask;

        emailQuota.LastReminder = _clock.UtcNow;
        _quotaService.SaveQuota(emailQuota);

        // Using deferred task to send the email after the current transaction is committed.
        ShellScope.AddDeferredTask(async _ =>
        {
            var emailParameters = await _emailQuotaEmailService.CreateEmailForExceedingQuotaAsync();
            emailParameters.Body = await _emailTemplateService.RenderEmailTemplateAsync("EmailQuote", new
            {
                HostName = _shellSettings.Name,
            });

            await _smtpService.SendAsync(emailParameters);
        });

        return Task.CompletedTask;
    }

    private static bool IsSameMonth(DateTime date1, DateTime date2) =>
        date1.Month == date2.Month && date1.Year == date2.Year;
}
