using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Security.Permissions.Permission;
using static OrchardCore.Security.StandardPermissions;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaEmailService : IEmailQuotaEmailService
{
    private readonly IRoleService _roleService;
    private readonly UserManager<IUser> _userManager;

    public EmailQuotaEmailService(
        IRoleService roleService,
        UserManager<IUser> userManager)
    {
        _roleService = roleService;
        _userManager = userManager;
    }

    public async Task<IEnumerable<string>> CollectUserEmailsForExceedingQuotaAsync()
    {
        // Get users with site owner permission.
        var roles = await _roleService.GetRolesAsync();
        var siteOwnerRoles = roles.Where(role =>
            (role as Role)?.RoleClaims.Exists(claim =>
                claim.ClaimType == ClaimType && claim.ClaimValue == SiteOwner.Name) == true);

        var siteOwners = new List<IUser>();
        foreach (var role in siteOwnerRoles)
        {
            siteOwners.AddRange(await _userManager.GetUsersInRoleAsync(role.RoleName));
        }

        return siteOwners.Select(user => (user as User)?.Email);
    }
}
