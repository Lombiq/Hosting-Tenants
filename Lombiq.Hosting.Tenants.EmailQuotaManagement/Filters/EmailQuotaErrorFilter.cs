using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.AdminDashboard.Controllers;
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
        var actionRouteValue = context.ActionDescriptor.RouteValues["Action"];

        if (actionRouteController == typeof(DashboardController).ControllerName() &&
            actionRouteValue is nameof(DashboardController.Index) &&
            context.Result is ViewResult &&
            await _quotaService.IsQuotaOverTheLimitAsync())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];

            await contentZone.AddAsync(await _shapeFactory.CreateAsync("EmailQuotaError"), "0");
        }

        await next();
    }
}
