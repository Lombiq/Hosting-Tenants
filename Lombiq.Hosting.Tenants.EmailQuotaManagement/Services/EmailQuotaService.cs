using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.Security.Permissions.Permission;
using static OrchardCore.Security.StandardPermissions;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaService : IEmailQuotaService
{
    private readonly ISession _session;
    private readonly EmailQuotaOptions _emailQuotaOptions;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly SmtpSettings _smtpOptions;
    private readonly IClock _clock;
    private readonly IRoleService _roleService;
    private readonly UserManager<IUser> _userManager;

    public EmailQuotaService(
        ISession session,
        IOptions<EmailQuotaOptions> emailQuotaOptions,
        IShellConfiguration shellConfiguration,
        IOptions<SmtpSettings> smtpOptions,
        IClock clock,
        IRoleService roleService,
        UserManager<IUser> userManager)
    {
        _session = session;
        _emailQuotaOptions = emailQuotaOptions.Value;
        _shellConfiguration = shellConfiguration;
        _smtpOptions = smtpOptions.Value;
        _clock = clock;
        _roleService = roleService;
        _userManager = userManager;
    }

    public async Task<IEnumerable<string>> CollectUserEmailsForEmailReminderAsync()
    {
        // Get users with site owner permission.
        var roles = await _roleService.GetRolesAsync();
        var siteOwnerRoles = roles.Where(role =>
            (role as Role)?.RoleClaims.Exists(claim =>
                claim.ClaimType == ClaimType && claim.ClaimValue == SiteOwner.Name) == true);

        var siteOwners = new List<IUser>();
        foreach (var role in siteOwnerRoles)
        {
            siteOwners.AddRange(await _userManager.GetUsersInRoleAsync(role.RoleName));
        }

        return siteOwners.Select(user => (user as User)?.Email);
    }

    public bool ShouldLimitEmails()
    {
        var originalHost = _shellConfiguration.GetValue<string>("SmtpSettings:Host");
        return originalHost == _smtpOptions.Host;
    }

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
        var currentQuota = await _session.Query<EmailQuota, EmailQuotaIndex>().FirstOrDefaultAsync();

        if (currentQuota != null) return currentQuota;

        currentQuota = new EmailQuota
        {
            // Need to set default value otherwise the database might complain about being 01/01/0001 out of range.
            LastReminderUtc = _clock.UtcNow.AddMonths(-1),
        };
        _session.Save(currentQuota);

        return currentQuota;
    }

    public void IncreaseEmailUsage(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailUsageCount++;
        _session.Save(emailQuota);
    }

    public void SetQuotaOnEmailReminder(EmailQuota emailQuota)
    {
        emailQuota.LastReminderUtc = _clock.UtcNow;
        emailQuota.LastReminderPercentage = emailQuota.CurrentUsagePercentage(GetEmailQuotaPerMonth());
        _session.Save(emailQuota);
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

    public void ResetQuota(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailUsageCount = 0;
        _session.Save(emailQuota);
    }

    public int GetEmailQuotaPerMonth() =>
        _emailQuotaOptions.EmailQuotaPerMonth;

    private static bool IsSameMonth(DateTime date1, DateTime date2) =>
        date1.Month == date2.Month && date1.Year == date2.Year;
}
