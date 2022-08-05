﻿using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Middlewares;

public class IdleTimeProviderMiddleware
{
    private readonly RequestDelegate _next;

    public IdleTimeProviderMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context, ILastActiveTimeAccessor lastActiveTimeAccessor)
    {
        lastActiveTimeAccessor.Update(context.RequestServices.GetService<IClock>());

        return _next(context);
    }
}
