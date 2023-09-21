using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Management.Permissions;

public class ShellSettingsEditPermissions : AdminPermissionBase
{
    public static readonly Permission ShellSettingsEditPermission =
        new(nameof(ShellSettingsEditPermission), "Able to edit shell settings in the tenant editor.");

    protected override IEnumerable<Permission> AdminPermissions => new[] { ShellSettingsEditPermission };
}
