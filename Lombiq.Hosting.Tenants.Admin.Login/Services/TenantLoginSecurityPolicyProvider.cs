using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Lombiq.Hosting.Tenants.Admin.Login.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Admin.Login.Services;

internal sealed class TenantLoginSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IShellHost _shellHost;

    public TenantLoginSecurityPolicyProvider(IActionContextAccessor actionContextAccessor, IShellHost shellHost)
    {
        _actionContextAccessor = actionContextAccessor;
        _shellHost = shellHost;
    }

    public ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        var actionContext = _actionContextAccessor.ActionContext;

        if (!TenantsIndexFilter.IsTenantsEditAction(actionContext))
        {
            return ValueTask.CompletedTask;
        }

        var shellName = actionContext.RouteData.Values["Id"].ToString();

        if (_shellHost.TryGetSettings(shellName, out var shellSettings))
        {
            CspHelper.MergeValues(securityPolicies, ContentSecurityPolicyDirectives.FormAction, shellSettings.RequestUrlHosts);
        }

        return ValueTask.CompletedTask;
    }
}
