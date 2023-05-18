using Lombiq.Hosting.Tenants.SearchBotBlocker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.SearchBotBlocker.Middlewares;

public class SearchBotBlockerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IOptions<SearchBotBlockerOptions> _options;

    public SearchBotBlockerMiddleware(
        RequestDelegate next,
        IHostEnvironment hostEnvironment,
        IOptions<SearchBotBlockerOptions> options)
    {
        _next = next;
        _hostEnvironment = hostEnvironment;
        _options = options;
    }

    public Task InvokeAsync(HttpContext context)
    {
        bool isProduction = _options.Value.IsProduction ?? _hostEnvironment.IsProduction();

        if (!isProduction)
        {
            context.Response.Headers.Add("X-Robots-Tag", "noindex, nofollow");
        }

        return _next(context);
    }
}
