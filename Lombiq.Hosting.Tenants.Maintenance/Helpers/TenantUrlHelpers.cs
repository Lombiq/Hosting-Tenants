using OrchardCore.Environment.Shell;

namespace Lombiq.Hosting.Tenants.Maintenance.Helpers;

internal static class TenantUrlHelpers
{
    public static string ReplaceTenantName(string url, string tenantName) =>
        // Evaluate the tenant name in lowercase as it will be used in the URL or request URL prefixes.
#pragma warning disable CA1308 // Normalize strings to uppercase
        url?.Replace("{TenantName}", tenantName.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase

    public static string GetEvaluatedValueForTenant(
        string valueForDefaultTenant,
        string valueForAnyTenant,
        ShellSettings shellSettings)
    {
        var evaluatedValue = !string.IsNullOrEmpty(valueForAnyTenant)
            ? ReplaceTenantName(valueForAnyTenant, shellSettings.Name)
            : string.Empty;

        return shellSettings.IsDefaultShell() ? valueForDefaultTenant : evaluatedValue;
    }
}
