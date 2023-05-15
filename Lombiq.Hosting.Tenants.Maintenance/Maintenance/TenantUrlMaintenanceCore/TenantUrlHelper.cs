using OrchardCore.Environment.Shell;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.TenantUrlMaintenanceCore;

internal static class TenantUrlHelper
{
    public static string GetTenantUrl(TenantUrlMaintenanceOptions options, ShellSettings shellSettings)
    {
        // The tenant name is intentionally lowercased here.
#pragma warning disable CA1308 // Normalize strings to uppercase
        var evaluatedTenantUrl = options.TenantUrl.Replace(
            "{TenantName}",
            shellSettings.Name.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase
        var defaultTenantUrl =
            string.IsNullOrEmpty(options.DefaultTenantUrl)
                ? evaluatedTenantUrl
                : options.DefaultTenantUrl;
        return shellSettings.IsDefaultShell()
            ? defaultTenantUrl
            : evaluatedTenantUrl;
    }
}
