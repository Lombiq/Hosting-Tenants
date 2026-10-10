using OrchardCore.Data.Migration;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public sealed class EmailQuotaMigrations : DataMigration
{
    /// <summary>
    /// Gets the latest version of the migration, used in the <see cref="Create"/> and in the latest
    /// <c>UpdateFromNAsync</c> method.
    /// </summary>
    public int LatestVersion { get; } = 3;

    public int Create() => LatestVersion;

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterTableAsync("EmailQuotaIndex", table => table
            .AddColumn<int>("LastReminderPercentage")
        );

        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        // Deleting index because it is not needed.
        await SchemaBuilder.DropTableAsync("EmailQuotaIndex");

        return LatestVersion;
    }
}
