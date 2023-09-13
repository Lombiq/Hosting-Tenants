using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaService : ISmtpService
{
    private readonly EmailQuotaOptions _emailQuotaOptions;
    private readonly IStringLocalizer<EmailQuotaService> T;
    private readonly ISmtpService _smtpService;
    private readonly IQuotaService _quotaService;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly SmtpSettings _smtpOptions;

    public EmailQuotaService(
        ISmtpService smtpService,
        IOptions<EmailQuotaOptions> emailQuotaOptions,
        IOptions<SmtpSettings> smtpOptions,
        IStringLocalizer<EmailQuotaService> stringLocalizer,
        IQuotaService quotaService,
        IShellConfiguration shellConfiguration)
    {
        _smtpService = smtpService;
        _emailQuotaOptions = emailQuotaOptions.Value;
        _smtpOptions = smtpOptions.Value;
        T = stringLocalizer;
        _quotaService = quotaService;
        _shellConfiguration = shellConfiguration;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var shellConfiguration = (SmtpSettings)_shellConfiguration.GetSection("SmtpSettings");
        if (shellConfiguration.DefaultSender != _smtpOptions.DefaultSender)
        {
            return await _smtpService.SendAsync(message);
        }

        var currentQuota = await _quotaService.GetCurrentQuotaAsync();
        if (_emailQuotaOptions.EmailQuota < currentQuota.CurrentEmailQuotaCount)
        {
            return SmtpResult.Failed(T["The email quota ({currentQuota}) for the site has been exceeded.", _emailQuotaOptions.EmailQuota]);
        }

        var emailResult = await _smtpService.SendAsync(message);
        if (emailResult == SmtpResult.Success)
        {
            _quotaService.IncreaseQuota(currentQuota);
        }

        return emailResult;
    }
}
