using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using OrchardCore.Data.Migration;
using System.Data.Common;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public class EmailQuotaMigrations : DataMigration
{
    public int UpdateFrom1()
    {
        SchemaBuilder.AlterTable(nameof(EmailQuotaIndex), table => table
            .AddColumn<int>(nameof(EmailQuotaIndex.LastReminderPercentage))
        );

        return 2;
    }

    public int UpdateFrom2()
    {
        // Renaming for safety reasons.
        TryRenameColumn("LastReminder", nameof(EmailQuotaIndex.LastReminderUtc));
        TryRenameColumn("CurrentEmailQuotaCount", nameof(EmailQuotaIndex.CurrentEmailUsageCount));

        // Deleting index because it is not needed.
        SchemaBuilder.DropTable(nameof(EmailQuotaIndex));

        return 3;
    }

    private void TryRenameColumn(string columnName, string newName)
    {
        try
        {
            SchemaBuilder.AlterTable(nameof(EmailQuotaIndex), table => table
                .RenameColumn(columnName, newName)
            );
        }
        catch (DbException)
        {
            // This is intentionally blank. If the column doesn't exist it means it already has the new name.
        }
    }
}
