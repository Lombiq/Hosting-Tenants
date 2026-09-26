using Lombiq.Hosting.Tenants.HealthChecks.Models;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
using YesSql.Sql;

namespace Lombiq.Hosting.Tenants.HealthChecks.Migrations;

public sealed class TenantHealthMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<TenantHealthIndex>(table => table
            .Column<string>(nameof(TenantHealthIndex.TenantName))
            .Column<bool>(nameof(TenantHealthIndex.IsHealthy))
        );

        return 1;
    }
}
