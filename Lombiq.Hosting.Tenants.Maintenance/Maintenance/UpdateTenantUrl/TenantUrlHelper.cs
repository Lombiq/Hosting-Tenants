using OrchardCore.Environment.Shell;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateTenantUrl;

internal static class TenantUrlHelper
{
    public static string GetTenantUrl(UpdateTenantUrlMaintenanceOptions options, ShellSettings shellSettings)
    {
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
