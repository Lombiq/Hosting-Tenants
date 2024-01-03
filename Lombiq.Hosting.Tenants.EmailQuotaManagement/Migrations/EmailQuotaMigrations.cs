using OrchardCore.Data.Migration;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public class EmailQuotaMigrations : DataMigration
{
    // This is actually needed like this, otherwise it won't work. CA1822 will only be violated during CI builds for
    // some reason.
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable S3400 // Methods should not return constants
#pragma warning disable CA1822 // Mark members as static
    public int Create() => 3;
#pragma warning restore S3400 // Methods should not return constants
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0079 // Remove unnecessary suppression

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
