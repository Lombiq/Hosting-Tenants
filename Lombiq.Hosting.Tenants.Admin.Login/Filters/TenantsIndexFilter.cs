using Lombiq.Hosting.Tenants.Admin.Login.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Admin.Login.Filters;

public class TenantsIndexFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;
    private readonly IShellHost _shellHost;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public TenantsIndexFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory,
        IShellHost shellHost,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
        _shellHost = shellHost;
        _hca = hca;
        _authorizationService = authorizationService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var actionRouteController = context.ActionDescriptor.RouteValues["Controller"];
        var actionRouteArea = context.ActionDescriptor.RouteValues["Area"];
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(AdminController).ControllerName() &&
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}" &&
            actionRouteValue is nameof(AdminController.Edit) &&
            context.Result is ViewResult &&
            await _authorizationService.AuthorizeAsync(
                _hca.HttpContext.User,
                TenantAdminPermissions.LoginAsAdmin)
            )
        {
            var shellSettings = _shellHost.GetSettings(context.RouteData.Values["Id"].ToString());
            if (shellSettings != null &&
                shellSettings.State == TenantState.Running &&
                !shellSettings.Name.EqualsOrdinalIgnoreCase(ShellSettings.DefaultShellName))
            {
                var layout = await _layoutAccessor.GetLayoutAsync();
                var contentZone = layout.Zones["Content"];
                await contentZone.AddAsync(
                    await _shapeFactory.CreateAsync("TenantAdminShape", new
                    {
                        shellSettings.RequestUrlHost,
                        shellSettings.RequestUrlPrefix,
                    }),
                    "5");
            }
        }

        await next();
    }
}
