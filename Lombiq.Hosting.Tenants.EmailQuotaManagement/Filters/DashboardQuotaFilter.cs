using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;

public class DashboardQuotaFilter(
    IShapeFactory shapeFactory,
    ILayoutAccessor layoutAccessor,
    IEmailQuotaService emailQuotaService) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.IsAdmin())
        {
            await next();
            return;
        }

        if (context.Result is ViewResult &&
            emailQuotaService.ShouldLimitEmails())
        {
            var currentEmailQuota = await emailQuotaService.IsQuotaOverTheLimitAsync();

            var currentUsagePercentage = currentEmailQuota.EmailQuota
                .CurrentUsagePercentage(emailQuotaService.GetEmailQuotaPerMonth());

            if (currentUsagePercentage >= 80)
            {
                var layout = await layoutAccessor.GetLayoutAsync();
                var contentZone = layout.Zones["Messages"];
                await contentZone.AddAsync(
                    await shapeFactory.CreateAsync("DashboardQuotaMessage", new
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
