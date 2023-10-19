using OrchardCore.Data.Migration;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public class EmailQuotaMigrations : DataMigration
{
    // This is actually needed like this, otherwise it won't work.
#pragma warning disable S3400
    public int Create() => 3;
#pragma warning restore S3400

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
