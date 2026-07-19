using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lombiq.Hosting.Tenants.Maintenance.Helpers;

internal static class TenantUrlHelpers
{
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "The tenant name is lowercase in the URL or request URL prefixes")]
    public static string ReplaceTenantName(string url, string tenantName) =>
        url?.Replace("{TenantName}", tenantName.ToLowerInvariant());

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
