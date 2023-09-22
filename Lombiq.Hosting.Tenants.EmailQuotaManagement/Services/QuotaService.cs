using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class QuotaService : IQuotaService
{
    private readonly ISession _session;
    private readonly EmailQuotaOptions _emailQuotaOptions;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly SmtpSettings _smtpOptions;
    private readonly IClock _clock;

    public QuotaService(
        ISession session,
        IOptions<EmailQuotaOptions> emailQuotaOptions,
        IShellConfiguration shellConfiguration,
        IOptions<SmtpSettings> smtpOptions,
        IClock clock)
    {
        _session = session;
        _emailQuotaOptions = emailQuotaOptions.Value;
        _shellConfiguration = shellConfiguration;
        _smtpOptions = smtpOptions.Value;
        _clock = clock;
    }

    public bool ShouldLimitEmails()
    {
        var originalHost = _shellConfiguration.GetValue<string>("SmtpSettings:Host");
        return originalHost == _smtpOptions.Host;
    }

    public async Task<QuotaResult> IsQuotaOverTheLimitAsync()
    {
        var currentQuota = await GetCurrentQuotaAsync();
        return new QuotaResult
        {
            IsOverQuota = _emailQuotaOptions.EmailQuotaPerMonth <= currentQuota.CurrentEmailQuotaCount,
            EmailQuota = currentQuota,
        };
    }

    public async Task<EmailQuota> GetCurrentQuotaAsync()
    {
        var currentQuota = await _session.Query<EmailQuota, EmailQuotaIndex>().FirstOrDefaultAsync();

        if (currentQuota != null) return currentQuota;

        currentQuota = new EmailQuota
        {
            // Need to set default value otherwise the database might complain about being 01/01/0001 out of range.
            LastReminder = _clock.UtcNow.AddMonths(-1),
        };
        _session.Save(currentQuota);

        return currentQuota;
    }

    public void IncreaseQuota(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailQuotaCount++;
        SaveQuota(emailQuota);
    }

    public void SaveQuota(EmailQuota emailQuota) =>
        _session.Save(emailQuota);

    public void ResetQuota(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailQuotaCount = 0;
        _session.Save(emailQuota);
    }
}
