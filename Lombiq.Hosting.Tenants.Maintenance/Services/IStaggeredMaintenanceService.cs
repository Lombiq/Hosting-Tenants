using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public interface IStaggeredMaintenanceService
{
    Task RunScheduledMaintenanceForAllTenantAsync(bool newVersion = false, bool reset = false);
}
