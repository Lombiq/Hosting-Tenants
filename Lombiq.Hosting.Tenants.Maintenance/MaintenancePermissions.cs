using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Maintenance;

public class MaintenancePermissions : AdminPermissionBase
{
    public static readonly Permission ViewMaintenanceTaskExecutions =
        new(nameof(ViewMaintenanceTaskExecutions), "View Maintenance Task Executions.");

    public static readonly Permission StartStaggeredTenantWakeUp =
        new(nameof(StartStaggeredTenantWakeUp), "Start staggered tenant wake-up.");

    protected override IEnumerable<Permission> AdminPermissions =>
    [
        ViewMaintenanceTaskExecutions,
        StartStaggeredTenantWakeUp,
    ];
}
