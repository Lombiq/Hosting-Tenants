using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Filters;

public class LastActiveTimeUpdaterApiFilter : ActionFilterAttribute
{
    private readonly ILastActiveTimeAccessor _activeTimeAccessor;

    public LastActiveTimeUpdaterApiFilter(ILastActiveTimeAccessor activeTimeAccessor) =>
        _activeTimeAccessor = activeTimeAccessor;

    public override void OnActionExecuted(ActionExecutedContext context) =>
        _activeTimeAccessor.Update();
}