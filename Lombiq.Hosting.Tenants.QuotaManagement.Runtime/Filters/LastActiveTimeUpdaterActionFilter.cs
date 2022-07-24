using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Filters;

public class LastActiveTimeUpdaterActionFilter : IActionFilter
{
    private readonly ILastActiveTimeAccessor _activeTimeAccessor;

    public LastActiveTimeUpdaterActionFilter(ILastActiveTimeAccessor activeTimeAccessor) =>
        _activeTimeAccessor = activeTimeAccessor;

    public void OnActionExecuting(ActionExecutingContext context) => _activeTimeAccessor.Update();

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Not needed to implement.
    }
}
