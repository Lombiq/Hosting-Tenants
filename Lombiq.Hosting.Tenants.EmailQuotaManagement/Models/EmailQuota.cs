using System;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;

public class EmailQuota
{
    public int CurrentEmailUsageCount { get; set; }
    public DateTime LastReminderUtc { get; set; }
    public int LastReminderPercentage { get; set; }
}
