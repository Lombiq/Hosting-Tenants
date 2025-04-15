using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// This interface is used to handle events related to staggered maintenance.
/// </summary>
public interface IStaggeredMaintenanceEvents
{
    /// <summary>
    /// Called when the staggered maintenance process is starting.
    /// </summary>
    public Task StartingAsync(StaggeredMaintenancePart part) => Task.CompletedTask;

    /// <summary>
    /// Called when the staggered maintenance process is finished or after canceled.
    /// </summary>
    public Task FinishedAsync(StaggeredMaintenancePart part) => Task.CompletedTask;

    /// <summary>
    /// Called when the staggered maintenance process is canceled.
    /// </summary>
    public Task CanceledAsync(StaggeredMaintenancePart part) => Task.CompletedTask;
}
