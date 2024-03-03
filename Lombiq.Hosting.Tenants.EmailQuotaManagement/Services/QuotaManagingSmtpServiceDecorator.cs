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

public class QuotaManagingSmtpServiceDecorator : IEmailService
{
    private readonly IStringLocalizer<QuotaManagingSmtpServiceDecorator> T;
    private readonly IEmailService _emailService;
    private readonly IEmailQuotaService _emailQuotaService;
    private readonly ShellSettings _shellSettings;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailQuotaSubjectService _emailQuotaSubjectService;

    public QuotaManagingSmtpServiceDecorator(
        IEmailService emailService,
        IStringLocalizer<QuotaManagingSmtpServiceDecorator> stringLocalizer,
        IEmailQuotaService emailQuotaService,
        ShellSettings shellSettings,
        IEmailTemplateService emailTemplateService,
        IEmailQuotaSubjectService emailQuotaSubjectService)
    {
        _emailService = emailService;
        T = stringLocalizer;
        _emailQuotaService = emailQuotaService;
        _shellSettings = shellSettings;
        _emailTemplateService = emailTemplateService;
        _emailQuotaSubjectService = emailQuotaSubjectService;
    }

    public async Task<EmailResult> SendAsync(MailMessage message, string providerName = null)
    {
        if (!_emailQuotaService.ShouldLimitEmails())
        {
            return await _emailService.SendAsync(message, providerName);
        }

        var isQuotaOverResult = await _emailQuotaService.IsQuotaOverTheLimitAsync();
        await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota, providerName);

        // Should send the email if the quota is not over the limit.
        if (isQuotaOverResult.IsOverQuota)
        {
            return EmailResult.FailedResult(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await _emailService.SendAsync(message, providerName);
        if (emailResult == EmailResult.SuccessResult)
        {
            await _emailQuotaService.IncreaseEmailUsageAsync(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }

    private async Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota, string providerName)
    {
        var currentUsagePercentage = emailQuota.CurrentUsagePercentage(_emailQuotaService.GetEmailQuotaPerMonth());
        if (!_emailQuotaService.ShouldSendReminderEmail(emailQuota, currentUsagePercentage)) return;

        var siteOwnerEmails = (await _emailQuotaService.GetUserEmailsForEmailReminderAsync()).ToList();
        if (currentUsagePercentage >= 100)
        {
            await SendQuotaEmailAsync(
                emailQuota,
                siteOwnerEmails,
                "EmailQuotaExceededError",
                _emailQuotaSubjectService.GetExceededEmailSubject(),
                currentUsagePercentage,
                providerName);
            return;
        }

        await SendQuotaEmailAsync(
            emailQuota,
            siteOwnerEmails,
            $"EmailQuotaWarning",
            _emailQuotaSubjectService.GetWarningEmailSubject(currentUsagePercentage),
            currentUsagePercentage,
            providerName);
    }

    private Task SendQuotaEmailAsync(
        EmailQuota emailQuota,
        IEnumerable<string> siteOwnerEmails,
        string emailTemplateName,
        string subject,
        int percentage,
        string providerName)
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
                emailMessage.Body = await _emailTemplateService.RenderEmailTemplateAsync(emailTemplateName, new
                {
                    HostName = _shellSettings.Name,
                    Percentage = percentage,
                });
                // IEmailService must be used within this class otherwise it won't call the original implementation, but
                // loop back to here.
                await _emailService.SendAsync(emailMessage, providerName);
            });
        }

        return _emailQuotaService.SetQuotaOnEmailReminderAsync(emailQuota);
    }
}
