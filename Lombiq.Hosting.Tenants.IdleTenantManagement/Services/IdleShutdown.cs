using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

public class IdleShutdown : IIdleShutdown
{
    private readonly IOptions<IdleShutdownOptions> _options;
    private readonly ShellSettings _shellSettings;
    private readonly IClock _clock;
    private readonly ILastActiveTimeAccessor _lastActiveTimeAccessor;
    private readonly ILogger<IdleShutdown> _logger;
    private readonly IShellHost _shellHost;

    public IdleShutdown(
        IOptions<IdleShutdownOptions> options,
        ShellSettings shellSettings,
        IClock clock,
        ILastActiveTimeAccessor lastActiveTimeAccessor,
        ILogger<IdleShutdown> logger,
        IShellHost shellHost)
    {
        _options = options;
        _shellSettings = shellSettings;
        _clock = clock;
        _lastActiveTimeAccessor = lastActiveTimeAccessor;
        _logger = logger;
        _shellHost = shellHost;
    }

    public async Task ShutDownIdleTenantsAsync()
    {
        var maxIdleMinutes = _options.Value.MaxIdleMinutes;

#pragma warning disable S125 // Commenting out until testing is done.
        // if (maxIdleMinutes <= 0 || _shellSettings.IsDefaultShell()) return;
#pragma warning restore S125

        var lastActiveDateTimeUtc = _lastActiveTimeAccessor.LastActiveDateTimeUtc;

        if (lastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= _clock?.UtcNow)
        {
            _logger?.LogWarning("Shutting down tenant \"{ShellName}\" because of idle timeout.", _shellSettings.Name);

            await _shellHost.ReleaseShellContextAsync(_shellSettings, eventSource: false);
        }
    }
}
