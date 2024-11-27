using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Users;
using System.Threading.Tasks;
using static OrchardCore.OrchardCoreConstants.Roles;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddAdministratorRoleToUsersWithRole;

public class AddAdministratorRoleToUsersWithRoleMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<AddAdministratorRoleToUsersWithRoleMaintenanceOptions> _options;
    private readonly RoleManager<IRole> _roleManager;
    private readonly UserManager<IUser> _userManager;
    private readonly ILogger _logger;

    public AddAdministratorRoleToUsersWithRoleMaintenanceProvider(
        IOptions<AddAdministratorRoleToUsersWithRoleMaintenanceOptions> options,
        RoleManager<IRole> roleManager,
        UserManager<IUser> userManager,
        ILogger<AddAdministratorRoleToUsersWithRoleMaintenanceProvider> logger)
    {
        _options = options;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !string.IsNullOrEmpty(_options.Value.Role) &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        if (await _roleManager.FindByNameAsync(_options.Value.Role) is not Role role)
        {
            _logger.LogWarning("Role {Role} not found. The maintenance task will not be executed.", _options.Value.Role);
            return;
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.RoleName);

        foreach (var user in usersInRole)
        {
            if (await _userManager.IsInRoleAsync(user, Administrator)) continue;

            await _userManager.AddToRoleAsync(user, Administrator);
            _logger.LogInformation("The Administrator role has been added to the user {UserName}.", user.UserName);
        }
    }
}
