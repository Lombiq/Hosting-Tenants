using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Lombiq.Hosting.Tenants.Management.Filters;

public class ForbiddenTenantsFilter : IAsyncActionFilter
{
    private readonly IStringLocalizer T;
    private readonly IOptions<ForbiddenTenantsOptions> _forbiddenTenantsOptions;

    public ForbiddenTenantsFilter(
        IStringLocalizer<ForbiddenTenantsFilter> stringLocalizer,
        IOptions<ForbiddenTenantsOptions> forbiddenTenantsOptions)
    {
        _forbiddenTenantsOptions = forbiddenTenantsOptions;
        T = stringLocalizer;
    }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var routeValues = context.ActionDescriptor.RouteValues;

        if (!context.HttpContext.Request.Method.EqualsOrdinalIgnoreCase(Http.Post) ||
            !routeValues["Area"].EqualsOrdinalIgnoreCase($"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}") ||

            !((routeValues["Controller"].EqualsOrdinalIgnoreCase(typeof(AdminController).ControllerName()) &&
            (routeValues["Action"].EqualsOrdinalIgnoreCase(nameof(AdminController.Create)) ||
            routeValues["Action"].EqualsOrdinalIgnoreCase(nameof(AdminController.Edit)))) ||

            (routeValues["Controller"].EqualsOrdinalIgnoreCase(typeof(ApiController).ControllerName()) &&
            routeValues["Action"].EqualsOrdinalIgnoreCase(nameof(ApiController.Create)))))
        {
            return next();
        }

        var forbiddenRequestUrlHosts = _forbiddenTenantsOptions.Value.RequestUrlHosts;

        if (forbiddenRequestUrlHosts != null && forbiddenRequestUrlHosts.Any())
        {
            var requestUrlHost = context.HttpContext.Request.Form[nameof(CreateApiViewModel.RequestUrlHost)].ToString();
            var hosts = requestUrlHost.Split(',').Select(host => host.Trim());

            var unacceptableHostnames = hosts.Where(hostname => forbiddenRequestUrlHosts.Contains(hostname)).ToList();

            if (unacceptableHostnames.Count != 0)
            {
                var unacceptableHostnamesString = string.Join(", ", unacceptableHostnames);
                context.ModelState.AddModelError(
                    nameof(CreateApiViewModel.RequestUrlHost),
                    T.Plural(
                        unacceptableHostnames.Count,
                        $"{unacceptableHostnamesString} is a forbidden hostname.",
                        $"{unacceptableHostnamesString} are forbidden hostnames."));
            }
        }

        return next();
    }
}
