using Lombiq.Hosting.Tenants.EnvironmentRobots.Extensions;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Middlewares;

public class EnvironmentRobotsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IOptions<EnvironmentRobotsOptions> _options;

    public EnvironmentRobotsMiddleware(
        RequestDelegate next,
        IHostEnvironment hostEnvironment,
        IOptions<EnvironmentRobotsOptions> options)
    {
        _next = next;
        _hostEnvironment = hostEnvironment;
        _options = options;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (!_hostEnvironment.IsProductionWithConfiguration(_options))
        {
            context.Response.Headers.Add("X-Robots-Tag", "noindex, nofollow");
        }

        return _next(context);
    }
}
