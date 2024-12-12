using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// Implementations of this interface can be used to execute custom maintenance tasks.
/// </summary>
public interface ICustomMaintenanceHandler
{
    /// <summary>
    /// Executes the maintenance logic.
    /// </summary>
    Task ExecuteAsync(MaintenanceTaskExecutionContext context);
}
