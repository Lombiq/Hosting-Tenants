using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using System.Threading.Tasks;
using static OrchardCore.Security.Permissions.Permission;
using static OrchardCore.Security.StandardPermissions;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.AddSiteOwnerPermissionToRole;

public class AddSiteOwnerPermissionToRoleMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<AddSiteOwnerPermissionToRoleMaintenanceOptions> _options;
    private readonly RoleManager<IRole> _roleManager;

    public AddSiteOwnerPermissionToRoleMaintenanceProvider(
        IOptions<AddSiteOwnerPermissionToRoleMaintenanceOptions> options,
        RoleManager<IRole> roleManager)
    {
        _options = options;
        _roleManager = roleManager;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        if (await _roleManager.FindByNameAsync(_options.Value.Role) is not Role role) return;

        if (role.RoleClaims.Exists(claim => claim.ClaimType == ClaimType && claim.ClaimValue == SiteOwner.Name)) return;

        role.RoleClaims.Add(new RoleClaim { ClaimType = ClaimType, ClaimValue = SiteOwner.Name });

        await _roleManager.UpdateAsync(role);
    }
}
