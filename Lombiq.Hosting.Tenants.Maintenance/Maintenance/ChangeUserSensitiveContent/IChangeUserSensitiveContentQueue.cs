using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

/// <summary>
/// A storage for the <see cref="User"/> database IDs that have to be changed.
/// </summary>
public interface IChangeUserSensitiveContentQueue
{
    /// <summary>
    /// Gets the maximum number of users returned by <see cref="DequeueAsync"/>. To have the best performance, we are
    /// processing users in batches and then saving them.
    /// </summary>
    int BatchSize { get; }

    /// <summary>
    /// Adds the IDs from the provided <paramref name="users"/> to the queue.
    /// </summary>
    void Enqueue(IEnumerable<User> users);

    /// <summary>
    /// Gets a batch of users that still need to be changed from the queue.
    /// </summary>
    Task<ICollection<User>> DequeueAsync();
}
