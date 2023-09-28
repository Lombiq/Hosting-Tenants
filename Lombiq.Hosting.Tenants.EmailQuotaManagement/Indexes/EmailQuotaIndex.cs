using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;

public class EmailQuotaIndex : MapIndex
{
    public int CurrentEmailQuotaCount { get; set; }
    public DateTime LastReminder { get; set; }
    public int LastReminderPercentage { get; set; }
}

public class EmailQuotaIndexProvider : IndexProvider<EmailQuota>
{
    public override void Describe(DescribeContext<EmailQuota> context) =>
        context.For<EmailQuotaIndex>()
            .Map(emailQuota => new EmailQuotaIndex
            {
                CurrentEmailQuotaCount = emailQuota.CurrentEmailQuotaCount,
                LastReminder = emailQuota.LastReminder,
                LastReminderPercentage = emailQuota.LastReminderPercentage,
            });
}
