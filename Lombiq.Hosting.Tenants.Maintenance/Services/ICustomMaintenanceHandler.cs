using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public interface ICustomMaintenanceHandler
{
    Task ExecuteAsync(MaintenanceTaskExecutionContext context);
}
