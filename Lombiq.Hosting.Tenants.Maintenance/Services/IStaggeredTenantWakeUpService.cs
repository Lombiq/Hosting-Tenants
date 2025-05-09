using Lombiq.Hosting.Tenants.Maintenance.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

/// <summary>
/// This service is responsible for running the staggered tenant wake-up process.
/// </summary>
public interface IStaggeredTenantWakeUpService
{
    /// <summary>
    /// Starts the maintenance process for all running tenants (except the default one) in a staggered manner, with a
    /// delay between each batch of tenants.
    /// </summary>
    Task<StaggeredTenantWakeUpPart> RunStaggeredTenantWakeUpAsync(bool newVersion = false);

    /// <summary>
    /// Gets or creates the staggered tenant wake-up content item. This is done to avoid creating the staggered tenant wake-up
    /// content item multiple times. We are using a content item structure so we can easily have an editor for it.
    /// </summary>
    Task<ContentItem> GetOrCreateStaggeredTenantWakeUpSettingsAsync();
}
