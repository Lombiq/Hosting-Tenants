using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;
using EmailAdminController = OrchardCore.Email.Controllers.AdminController;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;

public class EmailSettingsQuotaFilter : IAsyncResultFilter
{
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IEmailQuotaService _emailQuotaService;

    public EmailSettingsQuotaFilter(
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

        var isEmailTestPage = context.IsMvcRoute(
            nameof(EmailAdminController.Test),
            typeof(EmailAdminController).ControllerName(),
            $"{nameof(OrchardCore)}.{nameof(OrchardCore.Email)}");

        if ((isEmailTestPage || context.IsSiteSettingsPage("email")) && _emailQuotaService.ShouldLimitEmails())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var contentZone = layout.Zones["Content"];

            var quota = await _emailQuotaService.GetOrCreateCurrentQuotaAsync();
            await contentZone.AddAsync(
                await _shapeFactory.CreateAsync("EmailSettingsQuotaMessage", new
                {
                    quota.CurrentEmailUsageCount,
                    EmailQuotaPerMonth = _emailQuotaService.GetEmailQuotaPerMonth(),
                }),
                "0");
        }

        await next();
    }
}
