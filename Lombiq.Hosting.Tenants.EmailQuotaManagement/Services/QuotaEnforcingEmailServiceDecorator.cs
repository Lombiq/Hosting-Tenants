using Lombiq.HelpfulExtensions.Extensions.Emails.Services;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class QuotaEnforcingEmailServiceDecorator : IEmailService
{
    private readonly IStringLocalizer<QuotaEnforcingEmailServiceDecorator> T;
    private readonly IEmailService _emailService;
    private readonly IEmailQuotaService _emailQuotaService;
    private readonly ShellSettings _shellSettings;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailQuotaSubjectService _emailQuotaSubjectService;

    public QuotaEnforcingEmailServiceDecorator(
        IStringLocalizer<QuotaEnforcingEmailServiceDecorator> stringLocalizer,
        IEmailService emailService,
        IEmailQuotaService emailQuotaService,
        ShellSettings shellSettings,
        IEmailTemplateService emailTemplateService,
        IEmailQuotaSubjectService emailQuotaSubjectService)
    {
        T = stringLocalizer;
        _emailService = emailService;
        _emailQuotaService = emailQuotaService;
        _shellSettings = shellSettings;
        _emailTemplateService = emailTemplateService;
        _emailQuotaSubjectService = emailQuotaSubjectService;
    }

    public async Task<EmailResult> SendAsync(MailMessage message, string providerName = null)
    {
        if (!await _emailQuotaService.ShouldEnforceEmailQuotaAsync(providerName))
        {
            return await _emailService.SendAsync(message, providerName);
        }

        var isQuotaOverResult = await _emailQuotaService.IsQuotaOverTheLimitAsync();
        await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota);

        // Should send the email if the quota is not over the limit.
        if (isQuotaOverResult.IsOverQuota)
        {
            return EmailResult.FailedResult(T["Your site has run out of the email quota for this month."]);
        }

        var emailResult = await _emailService.SendAsync(message, providerName);
        if (emailResult.Succeeded) await _emailQuotaService.IncreaseEmailUsageAsync(isQuotaOverResult.EmailQuota);

        return emailResult;
    }

    private async Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota)
    {
        var currentUsagePercentage = emailQuota.CurrentUsagePercentage(_emailQuotaService.GetEmailQuotaPerMonth());
        if (!_emailQuotaService.ShouldSendReminderEmail(emailQuota, currentUsagePercentage))
        {
            return;
        }

        var administratorEmails = (await _emailQuotaService.GetUserEmailsForEmailReminderAsync()).ToList();
        if (currentUsagePercentage >= 100)
        {
            await SendEmailQuotaReminderAsync(
                emailQuota,
                administratorEmails,
                "EmailQuotaExceededError",
                _emailQuotaSubjectService.GetExceededEmailSubject(),
                currentUsagePercentage);

            return;
        }

        await SendEmailQuotaReminderAsync(
            emailQuota,
            administratorEmails,
            "EmailQuotaWarning",
            _emailQuotaSubjectService.GetWarningEmailSubject(currentUsagePercentage),
            currentUsagePercentage);
    }

    private Task SendEmailQuotaReminderAsync(
        EmailQuota emailQuota,
        IEnumerable<string> administratorEmails,
        string emailTemplateName,
        string subject,
        int percentage)
    {
        var emailMessage = new MailMessage
        {
            Subject = subject,
        };
        foreach (var administratorEmail in administratorEmails)
        {
            ShellScope.AddDeferredTask(async _ =>
            {
                emailMessage.To = administratorEmail;
                emailMessage.HtmlBody = await _emailTemplateService.RenderEmailTemplateAsync(emailTemplateName, new
                {
                    HostName = _shellSettings.Name,
                    Percentage = percentage,
                });

                await _emailService.SendAsync(emailMessage);
            });
        }

        return _emailQuotaService.SetQuotaOnEmailReminderAsync(emailQuota);
    }
}
