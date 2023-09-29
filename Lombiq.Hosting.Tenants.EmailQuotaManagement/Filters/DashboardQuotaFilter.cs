using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin.Controllers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;

public class DashboardQuotaFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IEmailQuotaService _emailQuotaService;

    public DashboardQuotaFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IEmailQuotaService emailQuotaService)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _emailQuotaService = emailQuotaService;
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
            _emailQuotaService.ShouldLimitEmails())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];
            var currentEmailQuota = await _emailQuotaService.IsQuotaOverTheLimitAsync();

            var currentUsagePercentage = currentEmailQuota.EmailQuota
                .CurrentUsagePercentage(_emailQuotaService.GetEmailQuotaPerMonth());

            if (currentUsagePercentage >= 80)
            {
                await contentZone.AddAsync(
                    await _shapeFactory.CreateAsync("DashboardQuotaMessage", new
                    {
                        currentEmailQuota.IsOverQuota,
                        UsagePercentage = currentUsagePercentage,
                    }),
                    "0");
            }
        }

        await next();
    }
}
