using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Lombiq.Hosting.Tenants.Admin.Login.Extensions;
using Lombiq.Hosting.Tenants.Admin.Login.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Admin.Login.Filters;

public sealed class TenantsIndexFilter : IAsyncResultFilter
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
        if (context.IsNotFullViewRendering() ||
            !context.IsTenantEditRoute() ||
            !await _authorizationService.AuthorizeAsync(
                _hca.HttpContext.User,
                TenantAdminPermissions.LoginAsAdmin))
        {
            await next();
            return;
        }

        var shellSettings = _shellHost.GetSettings(context.RouteData.Values["Id"].ToString());
        if (shellSettings != null &&
            shellSettings.State == TenantState.Running &&
            !shellSettings.Name.EqualsOrdinalIgnoreCase(ShellSettings.DefaultShellName))
        {
            await _layoutAccessor.AddShapeToZoneAsync(
                "Content",
                await _shapeFactory.CreateAsync("TenantAdminShape", new
                {
                    shellSettings.RequestUrlHost,
                    shellSettings.RequestUrlPrefix,
                }),
                "5");
        }

        await next();
    }
}
