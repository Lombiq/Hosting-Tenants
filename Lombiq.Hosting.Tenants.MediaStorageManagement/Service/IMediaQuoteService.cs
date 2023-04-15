using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Service;

public interface IMediaQuoteService
{
    Task<long> GetRemainingMediaSpaceLeft();
}
