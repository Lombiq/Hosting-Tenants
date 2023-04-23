using Lombiq.Hosting.Tenants.Maintenance.Models;

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
    public Task<MaintenanceTaskExecutionData> GetLatestExecutionByMaintenanceIdAsync(string maintenanceId);

    /// <summary>
    /// Executes all maintenance tasks if needed.
    /// </summary>
    public Task ExecuteMaintenanceTasksAsync();
}
