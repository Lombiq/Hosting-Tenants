namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;

public class QuotaResult
{
    public EmailQuota EmailQuota { get; set; }
    public bool IsOverQuota { get; set; }
}
