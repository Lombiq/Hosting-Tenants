using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

public class ChangeUserSensitiveContentQueue : IChangeUserSensitiveContentQueue
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConcurrentQueue<long> _userIds = new();

    // Multiple batch sizes were tried and 15 seems to grant the best performance.
    public int BatchSize => 15;

    public int Count => _userIds.Count;

    public ChangeUserSensitiveContentQueue(IServiceScopeFactory serviceScopeFactory) =>
        _serviceScopeFactory = serviceScopeFactory;

    public void Enqueue(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            _userIds.Enqueue(user.Id);
        }
    }

    public Task<ICollection<User>> DequeueAsync()
    {
        var userIds = new List<long>(capacity: BatchSize);

        for (var i = 0; i < BatchSize && _userIds.TryDequeue(out var id); i++)
        {
            userIds.Add(id);
        }

        return userIds.Count > 0 ? DequeueInnerAsync(userIds) : Task.FromResult<ICollection<User>>([]);
    }

    private async Task<ICollection<User>> DequeueInnerAsync(List<long> userIds)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var users = await scope.ServiceProvider.GetRequiredService<ISession>().GetAsync<User>([.. userIds]);
        return users.AsList();
    }
}
