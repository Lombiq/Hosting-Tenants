using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.RemoveUsers;

public class RemoveUsersMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<RemoveUsersMaintenanceOptions> _options;
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;

    public RemoveUsersMaintenanceProvider(
        IOptions<RemoveUsersMaintenanceOptions> options,
        ISession session,
        UserManager<IUser> userManager)
    {
        _options = options;
        _session = session;
        _userManager = userManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var users = await _session.Query<User>().ListAsync();
        foreach (var user in users.Where(user =>
            user.Email.EndsWith($"@{_options.Value.EmailDomain}", StringComparison.InvariantCulture)))
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
