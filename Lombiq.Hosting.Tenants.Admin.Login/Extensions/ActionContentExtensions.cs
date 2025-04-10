using Microsoft.AspNetCore.Mvc;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Tenants.Controllers;

namespace Lombiq.Hosting.Tenants.Admin.Login.Extensions;

public static class ActionContentExtensions
{
    public static bool IsTenantEditRoute(this ActionContext context) =>
        context.IsMvcRoute(
            nameof(AdminController.Edit),
            typeof(AdminController).ControllerName(),
            $"{nameof(OrchardCore)}.{nameof(OrchardCore.Tenants)}");
}
