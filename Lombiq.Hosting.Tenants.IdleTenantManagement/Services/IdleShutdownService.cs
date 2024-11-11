using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

internal sealed class IdleShutdownService : IIdleShutdownService
{
    private readonly IOptions<IdleShutdownOptions> _options;
    private readonly ShellSettings _shellSettings;
    private readonly IClock _clock;
    private readonly ILastActiveTimeAccessor _lastActiveTimeAccessor;
    private readonly ILogger<IdleShutdownService> _logger;
    private readonly IShellHost _shellHost;

    public IdleShutdownService(
        IOptions<IdleShutdownOptions> options,
        ShellSettings shellSettings,
        IClock clock,
        ILastActiveTimeAccessor lastActiveTimeAccessor,
        ILogger<IdleShutdownService> logger,
        IShellHost shellHost)
    {
        _options = options;
        _shellSettings = shellSettings;
        _clock = clock;
        _lastActiveTimeAccessor = lastActiveTimeAccessor;
        _logger = logger;
        _shellHost = shellHost;
    }

    public async Task ShutDownTenantIfIdleAsync()
    {
        _logger.LogError("This is a deliberate error.");

        var maxIdleMinutes = _options.Value.MaxIdleMinutes;

        if (maxIdleMinutes <= 0 || _shellSettings.IsDefaultShell()) return;

        var lastActiveDateTimeUtc = _lastActiveTimeAccessor.LastActiveDateTimeUtc;

        if (lastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= _clock?.UtcNow)
        {
            _logger?.LogInformation("Shutting down tenant \"{ShellName}\" because of idle timeout.", _shellSettings.Name);

            await _shellHost.ReleaseShellContextAsync(_shellSettings, eventSource: false);
        }
    }
}
