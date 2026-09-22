using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.HealthChecks.Migrations;

public sealed class TenantHealthMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.AlterTableAsync("EmailQuotaIndex", table => table
            .AddColumn<int>("LastReminderPercentage")
        );

        return 1;
    }
}
