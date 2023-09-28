using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin.Controllers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;

public class EmailQuotaErrorFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IQuotaService _quotaService;

    public EmailQuotaErrorFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IQuotaService quotaService)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _quotaService = quotaService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.IsAdmin())
        {
            await next();
            return;
        }

        var actionRouteController = context.ActionDescriptor.RouteValues["Controller"];
        var actionRouteArea = context.ActionDescriptor.RouteValues["Area"];
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(AdminController).ControllerName() &&
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Admin)}" &&
            actionRouteValue is nameof(AdminController.Index) &&
            context.Result is ViewResult &&
            _quotaService.ShouldLimitEmails())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];
            var currentEmailQuota = await _quotaService.IsQuotaOverTheLimitAsync();

            var roundedCurrentPercentage = _quotaService.CurrentUsagePercentage(currentEmailQuota.EmailQuota) / 10 * 10;

            if (roundedCurrentPercentage >= 80)
            {
                await contentZone.AddAsync(
                    await _shapeFactory.CreateAsync("EmailQuotaError", new
                    {
                        currentEmailQuota.IsOverQuota,
                        UsagePercentage = roundedCurrentPercentage,
                    }),
                    "0");
            }
        }

        await next();
    }
}
