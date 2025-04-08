using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public interface IStaggeredMaintenanceService
{
    Task<StaggeredMaintenancePart> RunScheduledMaintenanceForAllTenantAsync(bool newVersion = false, bool reset = false);
}
