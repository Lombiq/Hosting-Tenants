using Lombiq.HelpfulExtensions.Extensions.Emails.Services;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaService : ISmtpService
{
    private readonly IStringLocalizer<EmailQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly SmtpSettings _smtpOptions;
    private readonly IEnumerable<IEmailQoutaReachedHandler> _emailQoutaReachedHandlers;
    private readonly ILogger _logger;

    public EmailQuotaService(
        ISmtpService smtpService,
        IOptions<SmtpSettings> smtpOptions,
        IStringLocalizer<EmailQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IShellConfiguration shellConfiguration,
        IEnumerable<IEmailQoutaReachedHandler> emailQoutaReachedHandlers,
        ILogger<EmailQuotaService> logger)
    {
        _smtpService = smtpService;
        _smtpOptions = smtpOptions.Value;
        T = stringLocalizer;
        _quotaService = quotaService;
        _shellConfiguration = shellConfiguration;
        _emailQoutaReachedHandlers = emailQoutaReachedHandlers;
        _logger = logger;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var shellConfiguration = (SmtpSettings)_shellConfiguration.GetSection("SmtpSettings");
        if (shellConfiguration.DefaultSender != _smtpOptions.DefaultSender)
        {
            return await _smtpService.SendAsync(message);
        }

        var isQuotaOverResult = await _quotaService.IsQuotaOverTheLimitAsync();
        if (isQuotaOverResult.IsOverQuota)
        {
            await _emailQoutaReachedHandlers.InvokeAsync(handler => handler.HandleEmailQuotaReachedAsync(), _logger);
            return SmtpResult.Failed(T["The email quota for the site has been exceeded."]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _quotaService.IncreaseQuota(isQuotaOverResult.EmailQuota);
        }

        return emailResult;
    }
}
