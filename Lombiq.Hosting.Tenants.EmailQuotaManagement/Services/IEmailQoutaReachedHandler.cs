using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public interface IEmailQoutaReachedHandler
{
    Task HandleEmailQuotaReachedAsync();
}
