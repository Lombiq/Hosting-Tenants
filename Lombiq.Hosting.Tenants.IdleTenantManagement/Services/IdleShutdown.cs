using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

public class IdleShutdown(
    IOptions<IdleShutdownOptions> options,
    ShellSettings shellSettings,
    IClock clock,
    ILastActiveTimeAccessor lastActiveTimeAccessor,
    ILogger<IdleShutdown> logger,
    IShellHost shellHost) : IIdleShutdown
{
    public async Task ShutDownIdleTenantsAsync()
    {
        var maxIdleMinutes = options.Value.MaxIdleMinutes;

        if (maxIdleMinutes <= 0 || shellSettings.IsDefaultShell()) return;

        var lastActiveDateTimeUtc = lastActiveTimeAccessor.LastActiveDateTimeUtc;

        if (lastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= clock?.UtcNow)
        {
            logger?.LogInformation("Shutting down tenant \"{ShellName}\" because of idle timeout.", shellSettings.Name);

            await shellHost.ReleaseShellContextAsync(shellSettings, eventSource: false);
        }
    }
}
