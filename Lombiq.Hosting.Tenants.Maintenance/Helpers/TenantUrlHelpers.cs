namespace Lombiq.Hosting.Tenants.Maintenance.Helpers;

internal static class TenantUrlHelpers
{
    public static string ReplaceTenantName(string url, string tenantName) =>
        // Evaluate the tenant name in lowercase as it will be used in the URL or request URL prefixes.
#pragma warning disable CA1308 // Normalize strings to uppercase
        url?.Replace("{TenantName}", tenantName.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase
}
