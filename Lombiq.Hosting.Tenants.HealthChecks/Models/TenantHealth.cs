using OrchardCore.Entities;
using YesSql.Indexes;

namespace Lombiq.Hosting.Tenants.HealthChecks.Models;

public class TenantHealth : Entity
{
    public string TenantName { get; set; }
    public bool IsHealthy { get; set; }
}

public class TenantHealthIndex : MapIndex
{
    public string TenantName { get; set; }
    public bool IsHealthy { get; set; }
}

public class TenantHealthIndexProvider : IndexProvider<TenantHealth>
{
    public override void Describe(DescribeContext<TenantHealth> context) =>
        context.For<TenantHealthIndex>().Map(item =>
            new TenantHealthIndex
            {
                TenantName = item.TenantName,
                IsHealthy = item.IsHealthy,
            });
}
