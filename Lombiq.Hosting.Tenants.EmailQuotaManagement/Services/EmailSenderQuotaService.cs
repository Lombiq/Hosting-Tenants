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

public class EmailSenderQuotaService : ISmtpService
{
    private readonly IStringLocalizer<EmailSenderQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;
    private readonly IEmailQuotaEmailService _emailQuotaEmailService;
    private readonly ShellSettings _shellSettings;
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailSenderQuotaService(
        ISmtpService smtpService,
        IStringLocalizer<EmailSenderQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IEmailQuotaEmailService emailQuotaEmailService,
        ShellSettings shellSettings,
        IEmailTemplateService emailTemplateService)
    {
        _smtpService = smtpService;
        T = stringLocalizer;
        _quotaService = quotaService;
        _emailQuotaEmailService = emailQuotaEmailService;
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
        await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota);

        // Should send the email if the quota is not over the limit.
        if (isQuotaOverResult.IsOverQuota)
        {
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
        var currentUsagePercentage = _quotaService.CurrentUsagePercentage(emailQuota);
        if (!_quotaService.ShouldSendReminderEmail(emailQuota, currentUsagePercentage)) return;

        var siteOwnerEmails = (await _emailQuotaEmailService.CollectUserEmailsForExceedingQuotaAsync()).ToList();
        if (currentUsagePercentage >= 100)
        {
            var emailMessage = new MailMessage
            {
                Subject = T["[Action Required] Your DotNest site has run over its e-mail quota"],
                IsHtmlBody = true,
            };
            SendQuotaEmail(siteOwnerEmails, emailMessage, "EmailQuota", currentUsagePercentage);
            _quotaService.SaveQuotaReminder(emailQuota);
            return;
        }

        SendQuotaEmailWithPercentage(siteOwnerEmails, currentUsagePercentage / 10 * 10);
        _quotaService.SaveQuotaReminder(emailQuota);
    }

    private void SendQuotaEmailWithPercentage(
        IEnumerable<string> siteOwnerEmails,
        int percentage)
    {
        var emailMessage = new MailMessage
        {
            Subject = T["[Warning] Your DotNest site has used {0}% of its e-mail quota", percentage],
            IsHtmlBody = true,
        };
        SendQuotaEmail(siteOwnerEmails, emailMessage, $"EmailQuotaWarning", percentage);
    }

    private void SendQuotaEmail(
        IEnumerable<string> siteOwnerEmails,
        MailMessage emailMessage,
        string emailTemplateName,
        int percentage)
    {
        foreach (var siteOwnerEmail in siteOwnerEmails)
        {
            ShellScope.AddDeferredTask(async _ =>
            {
                emailMessage.To = siteOwnerEmail;
                emailMessage.Body = await _emailTemplateService.RenderEmailTemplateAsync(emailTemplateName, new
                {
                    HostName = _shellSettings.Name,
                    Percentage = percentage,
                });
                // ISmtpService must be used within this class otherwise it won't call the original ISmtpService
                // implementation, but loop back to here.
                await _smtpService.SendAsync(emailMessage);
            });
        }
    }


}
