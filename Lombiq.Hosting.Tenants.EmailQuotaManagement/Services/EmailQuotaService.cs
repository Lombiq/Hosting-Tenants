using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore;
using OrchardCore.Email;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Modules;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

// As noted under the GitHub issue https://github.com/OrchardCMS/OrchardCore/issues/15290#issuecomment-2093363619
// SaveAsync calls could be removed due to redundancy, however, removing them actually breaks functionality.
public class EmailQuotaService : IEmailQuotaService
{
    private readonly ISession _session;
    private readonly EmailQuotaOptions _emailQuotaOptions;
    private readonly IClock _clock;
    private readonly UserManager<IUser> _userManager;
    private readonly IEmailProviderResolver _emailProviderResolver;

    public EmailQuotaService(
        ISession session,
        IOptions<EmailQuotaOptions> emailQuotaOptions,
        IClock clock,
        UserManager<IUser> userManager,
        IEmailProviderResolver emailProviderResolver)
    {
        _session = session;
        _emailQuotaOptions = emailQuotaOptions.Value;
        _clock = clock;
        _userManager = userManager;
        _emailProviderResolver = emailProviderResolver;
    }

    public async Task<IEnumerable<string>> GetUserEmailsForEmailReminderAsync()
    {
        var administratorUsers = await _userManager.GetUsersInRoleAsync(OrchardCoreConstants.Roles.Administrator);
        return administratorUsers.Select(user => (user as User)?.Email);
    }

    public async Task<bool> ShouldLimitEmailsAsync(string providerName = null) =>
        await _emailProviderResolver.GetAsync(providerName) switch
        {
            DefaultSmtpEmailProvider => true,
            DefaultAzureEmailProvider => true,
            _ => false,
        };

    public async Task<QuotaResult> IsQuotaOverTheLimitAsync()
    {
        var currentQuota = await GetOrCreateCurrentQuotaAsync();
        return new QuotaResult
        {
            IsOverQuota = _emailQuotaOptions.EmailQuotaPerMonth <= currentQuota.CurrentEmailUsageCount,
            EmailQuota = currentQuota,
        };
    }

    public async Task<EmailQuota> GetOrCreateCurrentQuotaAsync()
    {
        var currentQuota = await _session.Query<EmailQuota>().FirstOrDefaultAsync();

        if (currentQuota != null) return currentQuota;

        currentQuota = new EmailQuota
        {
            // Need to set default value otherwise the database might complain about being 01/01/0001 out of range.
            LastReminderUtc = _clock.UtcNow.AddMonths(-1),
        };
        await _session.SaveAsync(currentQuota);

        return currentQuota;
    }

    public Task IncreaseEmailUsageAsync(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailUsageCount++;
        return _session.SaveAsync(emailQuota);
    }

    public Task SetQuotaOnEmailReminderAsync(EmailQuota emailQuota)
    {
        emailQuota.LastReminderUtc = _clock.UtcNow;
        emailQuota.LastReminderPercentage = emailQuota.CurrentUsagePercentage(GetEmailQuotaPerMonth());
        return _session.SaveAsync(emailQuota);
    }

    public bool ShouldSendReminderEmail(EmailQuota emailQuota, int currentUsagePercentage)
    {
        if (currentUsagePercentage < 80)
        {
            return false;
        }

        var isSameMonth = IsSameMonth(_clock.UtcNow, emailQuota.LastReminderUtc);

        if (!isSameMonth)
        {
            return true;
        }

        return !((emailQuota.LastReminderPercentage >= 80 && currentUsagePercentage < 90) ||
            (emailQuota.LastReminderPercentage >= 90 && currentUsagePercentage < 100) ||
            emailQuota.LastReminderPercentage >= 100);
    }

    public Task ResetQuotaAsync(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailUsageCount = 0;
        return _session.SaveAsync(emailQuota);
    }

    public int GetEmailQuotaPerMonth() =>
        _emailQuotaOptions.EmailQuotaPerMonth;

    private static bool IsSameMonth(DateTime date1, DateTime date2) =>
        date1.Month == date2.Month && date1.Year == date2.Year;
}
