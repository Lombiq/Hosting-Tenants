using Lombiq.Hosting.Tenants.EnvironmentRobots.Extensions;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Filters;

public class EnvironmentRobotsMetaTagFilter(
    IHostEnvironment hostEnvironment,
    IOptions<EnvironmentRobotsOptions> options,
    IResourceManager resourceManager) : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!hostEnvironment.IsProductionWithConfiguration(options))
        {
            resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "robots",
                Content = "noindex, nofollow",
            });
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Intentionally empty. Required by interface implementation only.
    }
}
