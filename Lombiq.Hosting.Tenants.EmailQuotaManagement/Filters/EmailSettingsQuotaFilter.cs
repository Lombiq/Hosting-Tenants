using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Queries.Controllers;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;

public class EmailSettingsQuotaFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IQuotaService _quotaService;
    private readonly EmailQuotaOptions _emailQuotaOptions;

    public EmailSettingsQuotaFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IQuotaService quotaService,
        IOptions<EmailQuotaOptions> emailQuotaOptions)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _quotaService = quotaService;
        _emailQuotaOptions = emailQuotaOptions.Value;
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
            actionRouteArea == $"{nameof(OrchardCore)}.{nameof(OrchardCore.Settings)}" &&
            actionRouteValue is nameof(AdminController.Index) &&
            context.Result is ViewResult &&
            context.RouteData.Values.TryGetValue("GroupId", out var groupId) &&
            (string)groupId == "email" &&
            _quotaService.ShouldLimitEmails())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];

            var quota = await _quotaService.GetCurrentQuotaAsync();
            await contentZone.AddAsync(
                await _shapeFactory.CreateAsync("EmailSettingsQuota", new
                {
                    CurrentEmailCount = quota.CurrentEmailQuotaCount,
                    _emailQuotaOptions.EmailQuota,
                }),
                "0");
        }

        await next();
    }
}
