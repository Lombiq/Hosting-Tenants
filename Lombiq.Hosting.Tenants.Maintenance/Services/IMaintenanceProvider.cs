using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// A provider for a particular maintenance task.
/// </summary>
public interface IMaintenanceProvider
{
    /// <summary>
    /// Gets the ID of the maintenance task.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the order of the maintenance task. The lower the number the earlier the task will be executed.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Determines whether the maintenance task should be executed.
    /// </summary>
    /// <param name="context">Provides information about the current execution.</param>
    /// <returns><see langword="true"/> if the maintenance task should be executed, <see langword="false"/> otherwise.</returns>
    Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context);

    /// <summary>
    /// Executes the maintenance task.
    /// </summary>
    /// <param name="context">Provides information about the current execution.</param>
    Task ExecuteAsync(MaintenanceTaskExecutionContext context);
}
