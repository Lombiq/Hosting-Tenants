using Lombiq.Hosting.Tenants.Admin.Login.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Admin.Login.Filters;

public class TenantsIndexFilter(
    ILayoutAccessor layoutAccessor,
    IShapeFactory shapeFactory,
    IShellHost shellHost,
    IHttpContextAccessor hca,
    IAuthorizationService authorizationService) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var actionRouteController = context.ActionDescriptor.RouteValues["Controller"];
        var actionRouteArea = context.ActionDescriptor.RouteValues["Area"];
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(AdminController).ControllerName() &&
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}" &&
            actionRouteValue is nameof(AdminController.Edit) &&
            context.Result is ViewResult &&
            await authorizationService.AuthorizeAsync(
                hca.HttpContext.User,
                TenantAdminPermissions.LoginAsAdmin)
            )
        {
            var shellSettings = shellHost.GetSettings(context.RouteData.Values["Id"].ToString());
            if (shellSettings != null &&
                shellSettings.State == TenantState.Running &&
                !shellSettings.Name.EqualsOrdinalIgnoreCase(ShellSettings.DefaultShellName))
            {
                var layout = await layoutAccessor.GetLayoutAsync();
                var contentZone = layout.Zones["Content"];
                await contentZone.AddAsync(
                    await shapeFactory.CreateAsync("TenantAdminShape", new
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
