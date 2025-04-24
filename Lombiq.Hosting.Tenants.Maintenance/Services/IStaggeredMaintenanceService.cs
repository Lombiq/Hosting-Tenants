using Lombiq.Hosting.Tenants.Maintenance.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// This service is responsible for running the staggered maintenance process.
/// </summary>
public interface IStaggeredMaintenanceService
{
    /// <summary>
    /// Starts the staggered maintenance process. This will run the scheduled maintenance for all tenants except the
    /// default tenant. Starts the maintenance process for all tenants in a staggered manner, with a delay between each
    /// batch of tenants.
    /// </summary>
    Task<StaggeredMaintenancePart> RunScheduledMaintenanceForAllTenantAsync(bool newVersion = false, bool reset = false);

    /// <summary>
    /// Gets or creates the staggered maintenance content item. This is done to avoid creating the staggered maintenance
    /// content item multiple times. We are using a content item structure so we can easily have an editor for it.
    /// </summary>
    Task<ContentItem> GetOrCreateStaggeredMaintenanceAsync();
}
