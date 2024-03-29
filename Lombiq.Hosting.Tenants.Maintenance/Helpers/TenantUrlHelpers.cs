using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Linq;

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
        ShellSettings shellSettings,
        IDictionary<string, string> valueForTenantByName = null)
    {
        var evaluatedValue = string.Empty;

        if (!string.IsNullOrEmpty(valueForAnyTenant))
        {
            evaluatedValue = ReplaceTenantName(valueForAnyTenant, shellSettings.Name);
        }
        else if (valueForTenantByName?.Any() == true)
        {
            foreach (var pair in valueForTenantByName)
            {
                if (pair.Key == shellSettings.Name) evaluatedValue = pair.Value;
            }
        }

        return shellSettings.IsDefaultShell() ? valueForDefaultTenant : evaluatedValue;
    }
}
