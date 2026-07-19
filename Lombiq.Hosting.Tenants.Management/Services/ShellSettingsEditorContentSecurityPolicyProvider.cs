using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Lombiq.HelpfulLibraries.OrchardCore.Security;
using Lombiq.Hosting.Tenants.Management.Filters;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Services;

public class ShellSettingsEditorContentSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    public ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        if (ShellSettingsEditorFilter.Condition(context))
        {
            MonacoContentSecurityPolicyProvider.AddMonacoPolicies(securityPolicies);
        }

        return ValueTask.CompletedTask;
    }
}
