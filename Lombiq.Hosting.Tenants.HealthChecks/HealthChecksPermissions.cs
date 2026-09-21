using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.HealthChecks;

public class HealthChecksPermissions : AdminPermissionBase
{
    public static readonly Permission ViewHealthChecks =
        new(nameof(ViewHealthChecks), "View tenant Health Checks on the default tenant admin menu.");

    protected override IEnumerable<Permission> AdminPermissions =>
    [
        ViewHealthChecks,
    ];
}
