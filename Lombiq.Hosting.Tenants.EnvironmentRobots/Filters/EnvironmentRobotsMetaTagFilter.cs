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
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly IOptions<EnvironmentRobotsOptions> _options = options;
    private readonly IResourceManager _resourceManager = resourceManager;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!_hostEnvironment.IsProductionWithConfiguration(_options))
        {
            _resourceManager.RegisterMeta(new MetaEntry
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
