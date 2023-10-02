using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;

public class EmailQuotaIndex : MapIndex
{
    public int CurrentEmailUsageCount { get; set; }
    public DateTime LastReminderUtc { get; set; }
    public int LastReminderPercentage { get; set; }
}

public class EmailQuotaIndexProvider : IndexProvider<EmailQuota>
{
    public override void Describe(DescribeContext<EmailQuota> context) =>
        context.For<EmailQuotaIndex>()
            .Map(emailQuota => new EmailQuotaIndex
            {
                CurrentEmailUsageCount = emailQuota.CurrentEmailUsageCount,
                LastReminderUtc = emailQuota.LastReminderUtc,
                LastReminderPercentage = emailQuota.LastReminderPercentage,
            });
}
