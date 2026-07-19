using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Lombiq.Hosting.Tenants.Admin.Login.Extensions;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Admin.Login.Services;

internal sealed class TenantLoginSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    private readonly IShellHost _shellHost;

    public TenantLoginSecurityPolicyProvider(IShellHost shellHost) =>
        _shellHost = shellHost;

    public async ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        var actionContext = await context.GetActionContextAsync();
        if (actionContext?.IsTenantEditRoute() != true) return;

        var shellName = actionContext.RouteData.Values["Id"]?.ToString() ?? ShellSettings.DefaultShellName;

        if (_shellHost.TryGetSettings(shellName, out var shellSettings))
        {
            CspHelper.MergeValues(securityPolicies, ContentSecurityPolicyDirectives.FormAction, shellSettings.RequestUrlHosts);
        }
    }
}
