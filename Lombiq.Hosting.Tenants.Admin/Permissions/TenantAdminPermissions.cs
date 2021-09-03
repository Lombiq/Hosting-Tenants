using Lombiq.HelpfulLibraries.Libraries.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Admin.Permissions
{
    public class TenantAdminPermissions : AdminPermissionBase
    {
        public static readonly Permission LoginAsAdmin =
            new(nameof(LoginAsAdmin), "Able to login as an admin to any tenant from the Default tenant.");

        protected override IEnumerable<Permission> AdminPermissions => new[] { LoginAsAdmin };
    }
}
