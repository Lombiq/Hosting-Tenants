using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using OrchardCore.Data.Migration;
using System;
using YesSql.Sql;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;

public class EmailQuotaMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<EmailQuotaIndex>(
            table => table.Column<int>(nameof(EmailQuotaIndex.CurrentEmailQuotaCount))
                .Column<DateTime>(nameof(EmailQuotaIndex.LastReminder))
                .Column<int>(nameof(EmailQuotaIndex.LastReminderPercentage)));

        return 2;
    }

    public int UpdateFrom1()
    {
        SchemaBuilder.AlterTable(nameof(EmailQuotaIndex), table => table
            .AddColumn<int>(nameof(EmailQuotaIndex.LastReminderPercentage))
        );

        return 2;
    }
}
