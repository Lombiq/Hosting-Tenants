using Lombiq.Hosting.Tenants.EnvironmentRobots.Extensions;
using Lombiq.Hosting.Tenants.EnvironmentRobots.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
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
            var headerValue = context.Response.Headers["X-Robots-Tag"].FirstOrDefault() ?? string.Empty;

            var directives = new List<string>();

            if (!string.IsNullOrEmpty(headerValue))
            {
                directives.Add(headerValue);
            }

            // False warning, since headerValue is initialized to string.Empty if it would be null.
#pragma warning disable S2259 // Null pointers should not be dereferenced
            if (!headerValue.Contains("noindex"))
            {
                directives.Add("noindex");
            }
#pragma warning restore S2259 // Null pointers should not be dereferenced

            if (!headerValue.Contains("nofollow"))
            {
                directives.Add("nofollow");
            }

            if (directives.Count > 1)
            {
                context.Response.Headers["X-Robots-Tag"] = $"{string.Join(", ", directives)}";
            }
        }

        return _next(context);
    }
}
