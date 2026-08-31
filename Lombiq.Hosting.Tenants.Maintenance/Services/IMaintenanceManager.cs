#nullable enable

using Lombiq.Hosting.Tenants.Maintenance.Models;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// This service is responsible for executing maintenance tasks.
/// </summary>
public interface IMaintenanceManager
{
    /// <summary>
    /// Returns the latest execution of a maintenance task by its ID.
    /// </summary>
    /// <param name="maintenanceId">The ID of the maintenance task.</param>
    /// <returns>The latest execution of the maintenance task.</returns>
    public Task<MaintenanceTaskExecutionData?> GetLatestExecutionByMaintenanceIdAsync(string maintenanceId);

    /// <summary>
    /// Executes all maintenance tasks if needed.
    /// </summary>
    public Task ExecuteMaintenanceTasksAsync();

    /// <summary>
    /// Deletes all the executions of a maintenance task by its ID.
    /// </summary>
    /// <param name="maintenanceId">The ID of the maintenance task.</param>
    public Task DeleteMaintenanceExecutionsByIdAsync(string maintenanceId);

    /// <summary>
    /// Returns the maintenance provider by <see cref="IMaintenanceProvider.Id"/>, if it exists.
    /// </summary>
    IMaintenanceProvider? GetProviderById(string maintenanceId);

    /// <summary>
    /// Triggers execution of a specific maintenance task of a given <paramref name="provider"/> if <paramref
    /// name="forceExecute"/> is <see langword="true"/> or if <see cref="IMaintenanceProvider.ShouldExecuteAsync"/>
    /// evaluates to <see langword="true"/>.
    /// </summary>
    Task<MaintenanceTaskExecutionData?> ExecuteMaintenanceTaskIfNeededAsync(IMaintenanceProvider provider,
        MaintenanceTaskExecutionContext context,
        bool forceExecute = false);
}
