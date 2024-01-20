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

public class RemoveUsersMaintenanceProvider(
    IOptions<RemoveUsersMaintenanceOptions> options,
    ISession session,
    UserManager<IUser> userManager) : MaintenanceProviderBase
{
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var users = await session.Query<User>().ListAsync();
        foreach (var user in users.Where(user =>
            user.Email.EndsWith($"@{options.Value.EmailDomain}", StringComparison.InvariantCulture)))
        {
            await userManager.DeleteAsync(user);
        }
    }
}
