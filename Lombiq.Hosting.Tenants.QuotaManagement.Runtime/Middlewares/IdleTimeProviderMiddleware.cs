using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;
using Microsoft.AspNetCore.Http;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Middlewares;

public class IdleTimeProviderMiddleware
{
    private readonly RequestDelegate _next;

    public IdleTimeProviderMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context, ILastActiveTimeAccessor lastActiveTimeAccessor )
    {
        lastActiveTimeAccessor.Update();

        return _next(context);
    }
}
