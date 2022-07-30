using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using OrchardCore.Security.Permissions;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Permissions;

public class IdleMinutesPermissions : AdminPermissionBase
{
    public static readonly Permission ManageIdleMinutesSettings =
        new(nameof(IdleMinutesSettings), "Manage settings for the \"Maximum idle minutes\" feature");

    protected override IEnumerable<Permission> AdminPermissions { get; } = new[] { ManageIdleMinutesSettings, };
}
