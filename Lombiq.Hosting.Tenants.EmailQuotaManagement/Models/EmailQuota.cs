using System;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;

public class EmailQuota
{
    public int CurrentEmailUsageCount { get; set; }
    public DateTime LastReminderUtc { get; set; }
    public int LastReminderPercentage { get; set; }

    public int CurrentUsagePercentage(int emailQuotaPerMonth) =>
        Convert.ToInt32(Math.Round((double)CurrentEmailUsageCount / emailQuotaPerMonth * 100, 0));
}
