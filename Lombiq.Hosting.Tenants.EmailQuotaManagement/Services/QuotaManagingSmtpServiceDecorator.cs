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

public class QuotaManagingSmtpServiceDecorator(
    ISmtpService smtpService,
    IStringLocalizer<QuotaManagingSmtpServiceDecorator> stringLocalizer,
    IEmailQuotaService emailQuotaService,
    ShellSettings shellSettings,
    IEmailTemplateService emailTemplateService,
    IEmailQuotaSubjectService emailQuotaSubjectService) : ISmtpService
{
    private readonly IStringLocalizer T = stringLocalizer;

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        if (!emailQuotaService.ShouldLimitEmails())
        {
            return await smtpService.SendAsync(message);
        }

        var isQuotaOverResult = await emailQuotaService.IsQuotaOverTheLimitAsync();
        await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota);

        // Should send the email if the quota is not over the limit.
        if (isQuotaOverResult.IsOverQuota)
        {
            return SmtpResult.Failed(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            await emailQuotaService.IncreaseEmailUsageAsync(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }

    private async Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota)
    {
        var currentUsagePercentage = emailQuota.CurrentUsagePercentage(emailQuotaService.GetEmailQuotaPerMonth());
        if (!emailQuotaService.ShouldSendReminderEmail(emailQuota, currentUsagePercentage)) return;

        var siteOwnerEmails = (await emailQuotaService.GetUserEmailsForEmailReminderAsync()).ToList();
        if (currentUsagePercentage >= 100)
        {
            await SendQuotaEmailAsync(
                emailQuota,
                siteOwnerEmails,
                "EmailQuotaExceededError",
                emailQuotaSubjectService.GetExceededEmailSubject(),
                currentUsagePercentage);
            return;
        }

        await SendQuotaEmailAsync(
            emailQuota,
            siteOwnerEmails,
            $"EmailQuotaWarning",
            emailQuotaSubjectService.GetWarningEmailSubject(currentUsagePercentage),
            currentUsagePercentage);
    }

    private Task SendQuotaEmailAsync(
        EmailQuota emailQuota,
        IEnumerable<string> siteOwnerEmails,
        string emailTemplateName,
        string subject,
        int percentage)
    {
        var emailMessage = new MailMessage
        {
            IsHtmlBody = true,
            Subject = subject,
        };
        foreach (var siteOwnerEmail in siteOwnerEmails)
        {
            ShellScope.AddDeferredTask(async _ =>
            {
                emailMessage.To = siteOwnerEmail;
                emailMessage.Body = await emailTemplateService.RenderEmailTemplateAsync(emailTemplateName, new
                {
                    HostName = shellSettings.Name,
                    Percentage = percentage,
                });
                // ISmtpService must be used within this class otherwise it won't call the original ISmtpService
                // implementation, but loop back to here.
                await smtpService.SendAsync(emailMessage);
            });
        }

        return emailQuotaService.SetQuotaOnEmailReminderAsync(emailQuota);
    }
}
