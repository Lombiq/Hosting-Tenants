using System;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;

public class EmailQuota
{
    public int CurrentEmailQuotaCount { get; set; }
    public DateTime LastReminder { get; set; }
}
