using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// A provider for a particular maintenance task.
/// </summary>
public interface IMaintenanceProvider
{
    /// <summary>
    /// The ID of the maintenance task.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The order of the maintenance task. The lower the number the earlier the task will be executed.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Determines whether the maintenance task should be executed.
    /// </summary>
    /// <param name="context">Provides information about the current execution.</param>
    /// <returns>True if the maintenance task should be executed, false otherwise.</returns>
    Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context);

    /// <summary>
    /// Executes the maintenance task.
    /// </summary>
    /// <param name="context">Provides information about the current execution.</param>
    Task ExecuteAsync(MaintenanceTaskExecutionContext context);
}
