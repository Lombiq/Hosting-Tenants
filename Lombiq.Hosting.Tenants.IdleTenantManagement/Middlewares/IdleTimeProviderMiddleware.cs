using Lombiq.Hosting.Tenants.IdleTenantManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
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

public class IdleTenantEnabler
{
    private readonly RequestDelegate _next;
    private readonly IRunningShellTable _runningShellTable;
    private readonly IShellHost _shellHost;

    public IdleTenantEnabler(
        RequestDelegate next,
        IRunningShellTable runningShellTable,
        IShellHost shellHost)
    {
        _shellHost = shellHost;
        _runningShellTable = runningShellTable;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var httpRequest = context.Request;
        var shellSettings = _runningShellTable.Match(httpRequest.Host, httpRequest.Path);
        var prefix = httpRequest.Path.ToString().Trim('/');

        var settings = _shellHost
            .GetAllSettings()
            .SingleOrDefault(shell => shell.RequestUrlPrefix == prefix);

        if (shellSettings?.State == TenantState.Disabled)
        {
            shellSettings.State = TenantState.Running;
            await _shellHost.UpdateShellSettingsAsync(shellSettings);
        }

        if (settings is { State: TenantState.Disabled })
        {
            settings.State = TenantState.Running;
            await _shellHost.UpdateShellSettingsAsync(settings);
        }

        await _next(context);
    }
}
