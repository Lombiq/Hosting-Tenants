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

    private async Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota)
    {
        if (IsSameMonth(_clock.UtcNow, emailQuota.LastReminder)) return;

        emailQuota.LastReminder = _clock.UtcNow;
        _quotaService.SaveQuota(emailQuota);

        var siteOwnerEmails = await _emailQuotaEmailService.CollectUserEmailsForExceedingQuotaAsync();
        var emailMessage = new MailMessage
        {
            Subject = T["[Action Required] Your DotNest site has run over its e-mail quota"],
            IsHtmlBody = true,
        };

        foreach (var siteOwnerEmail in siteOwnerEmails)
        {
            ShellScope.AddDeferredTask(async _ =>
            {
                emailMessage.To = siteOwnerEmail;
                emailMessage.Body = await _emailTemplateService.RenderEmailTemplateAsync("EmailQuota", new
                {
                    HostName = _shellSettings.Name,
                });
                await _smtpService.SendAsync(emailMessage);
            });
        }
    }

    private static bool IsSameMonth(DateTime date1, DateTime date2) =>
        date1.Month == date2.Month && date1.Year == date2.Year;
}
