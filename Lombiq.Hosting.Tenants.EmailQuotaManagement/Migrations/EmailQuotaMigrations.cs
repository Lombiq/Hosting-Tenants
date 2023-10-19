using OrchardCore.Data.Migration;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public class EmailQuotaMigrations : DataMigration
{
    public int UpdateFrom1()
    {
        SchemaBuilder.AlterTable("EmailQuotaIndex", table => table
            .AddColumn<int>("LastReminderPercentage")
        );

        return 2;
    }

    public int UpdateFrom2()
    {
        // Deleting index because it is not needed.
        SchemaBuilder.DropTable("EmailQuotaIndex");

        return 3;
    }
}
