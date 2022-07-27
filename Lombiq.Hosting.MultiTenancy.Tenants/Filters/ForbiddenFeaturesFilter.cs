using Lombiq.Hosting.MultiTenancy.Tenants.Settings;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Filters;
public class ForbiddenFeaturesFilter //: IAsyncActionFilter
{
    private readonly IStringLocalizer T;
    private readonly IOptions<ForbiddenFeaturesOptions> _forbiddenFeaturesOptions;

    public ForbiddenFeaturesFilter(
        IStringLocalizer<ForbiddenFeaturesFilter> stringLocalizer,
        IOptions<ForbiddenFeaturesOptions> forbiddenFeaturesOptions)
    {
        _forbiddenFeaturesOptions = forbiddenFeaturesOptions;
        T = stringLocalizer;
    }

    //public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    //{
    //    return next(context);
    //}
}
