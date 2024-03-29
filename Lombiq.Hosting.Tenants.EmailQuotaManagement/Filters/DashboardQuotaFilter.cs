using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
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

        if (context.Result is ViewResult &&
            _emailQuotaService.ShouldLimitEmails())
        {
            var currentEmailQuota = await _emailQuotaService.IsQuotaOverTheLimitAsync();

            var currentUsagePercentage = currentEmailQuota.EmailQuota
                .CurrentUsagePercentage(_emailQuotaService.GetEmailQuotaPerMonth());

            if (currentUsagePercentage >= 80)
            {
                var layout = await _layoutAccessor.GetLayoutAsync();
                var contentZone = layout.Zones["Messages"];
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
