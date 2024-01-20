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

public class AddSiteOwnerPermissionToRoleMaintenanceProvider(
    IOptions<AddSiteOwnerPermissionToRoleMaintenanceOptions> options,
    RoleManager<IRole> roleManager) : MaintenanceProviderBase
{
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        if (await roleManager.FindByNameAsync(options.Value.Role) is not Role role) return;

        if (role.RoleClaims.Exists(claim => claim.ClaimType == ClaimType && claim.ClaimValue == SiteOwner.Name)) return;

        role.RoleClaims.Add(new RoleClaim { ClaimType = ClaimType, ClaimValue = SiteOwner.Name });

        await roleManager.UpdateAsync(role);
    }
}
