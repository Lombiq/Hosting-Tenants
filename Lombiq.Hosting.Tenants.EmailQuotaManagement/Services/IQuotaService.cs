using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public interface IQuotaService
{
    Task<EmailQuota> GetCurrentQuotaAsync();
    void IncreaseQuota(EmailQuota emailQuota);
    void ResetQuota(EmailQuota emailQuota);
}
