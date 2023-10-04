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

public class QuotaManagingSmtpServiceDecorator : ISmtpService
{
    private readonly IStringLocalizer<QuotaManagingSmtpServiceDecorator> T;
    private readonly ISmtpService _smtpService;
    private readonly IEmailQuotaService _emailQuotaService;
    private readonly ShellSettings _shellSettings;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailQuotaSubjectService _emailQuotaSubjectService;

    public QuotaManagingSmtpServiceDecorator(
        ISmtpService smtpService,
        IStringLocalizer<QuotaManagingSmtpServiceDecorator> stringLocalizer,
        IEmailQuotaService emailQuotaService,
        ShellSettings shellSettings,
        IEmailTemplateService emailTemplateService,
        IEmailQuotaSubjectService emailQuotaSubjectService)
    {
        _smtpService = smtpService;
        T = stringLocalizer;
        _emailQuotaService = emailQuotaService;
        _shellSettings = shellSettings;
        _emailTemplateService = emailTemplateService;
        _emailQuotaSubjectService = emailQuotaSubjectService;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        if (!_emailQuotaService.ShouldLimitEmails())
        {
            return await _smtpService.SendAsync(message);
        }

        var isQuotaOverResult = await _emailQuotaService.IsQuotaOverTheLimitAsync();
        await SendAlertEmailIfNecessaryAsync(isQuotaOverResult.EmailQuota);

        // Should send the email if the quota is not over the limit.
        if (isQuotaOverResult.IsOverQuota)
        {
            return SmtpResult.Failed(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _emailQuotaService.IncreaseEmailUsage(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }

    private Task SendAlertEmailIfNecessaryAsync(EmailQuota emailQuota)
    {
        var currentUsagePercentage = emailQuota.CurrentUsagePercentage(_emailQuotaService.GetEmailQuotaPerMonth());
        if (!_emailQuotaService.ShouldSendReminderEmail(emailQuota, currentUsagePercentage)) return Task.CompletedTask;

        if (currentUsagePercentage >= 100)
        {
            SendQuotaEmail(
                emailQuota,
                new[] {""},
                "EmailQuotaExceededError",
                _emailQuotaSubjectService.GetExceededEmailSubject(),
                currentUsagePercentage);
            return Task.CompletedTask;
        }

        SendQuotaEmail(
            emailQuota,
            new[] {""},
            $"EmailQuotaWarning",
            _emailQuotaSubjectService.GetWarningEmailSubject(currentUsagePercentage),
            currentUsagePercentage);
        return Task.CompletedTask;
    }

    private void SendQuotaEmail(
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

        _emailQuotaService.SetQuotaOnEmailReminder(emailQuota);
    }
}
