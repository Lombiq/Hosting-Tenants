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
    private readonly IEmailQuotaService _emailQuotaService;
    private readonly EmailQuotaOptions _emailQuotaOptions;

    public EmailSettingsQuotaFilter(
        IShapeFactory shapeFactory,
        ILayoutAccessor layoutAccessor,
        IEmailQuotaService emailQuotaService,
        IOptions<EmailQuotaOptions> emailQuotaOptions)
    {
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _emailQuotaService = emailQuotaService;
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
            _emailQuotaService.ShouldLimitEmails())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];

            var quota = await _emailQuotaService.GetOrCreateCurrentQuotaAsync();
            await contentZone.AddAsync(
                await _shapeFactory.CreateAsync("EmailSettingsQuotaMessage", new
                {
                    CurrentEmailCount = quota.CurrentEmailUsageCount,
                    EmailQuota = _emailQuotaOptions.EmailQuotaPerMonth,
                }),
                "0");
        }

        await next();
    }
}
