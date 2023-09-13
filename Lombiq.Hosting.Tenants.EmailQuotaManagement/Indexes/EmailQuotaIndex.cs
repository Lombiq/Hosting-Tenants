using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;

public class EmailQuotaIndex : MapIndex
{
    public int CurrentEmailQuotaCount { get; set; }
}

public class EmailQuotaIndexProvider : IndexProvider<EmailQuota>
{
    public override void Describe(DescribeContext<EmailQuota> context) =>
        context.For<EmailQuotaIndex>()
            .Map(emailQuota => new EmailQuotaIndex
            {
                CurrentEmailQuotaCount = emailQuota.CurrentEmailQuotaCount,
            });
}
