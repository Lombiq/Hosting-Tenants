using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Shut down idle tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    private readonly ILogger<IdleShutdownTask> _logger;
    private readonly IClock _clock;
    private readonly IOptions<IdleShutdownOptions> _options;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly ILastActiveTimeAccessor _lastActiveTimeAccessor;

    public IdleShutdownTask(
        ILogger<IdleShutdownTask> logger,
        IClock clock,
        IOptions<IdleShutdownOptions> options,
        IShellHost shellHost,
        ShellSettings shellSettings,
        ILastActiveTimeAccessor lastActiveTimeAccessor)
    {
        _logger = logger;
        _clock = clock;
        _options = options;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _lastActiveTimeAccessor = lastActiveTimeAccessor;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var maxIdleMinutes = _options.Value.MaxIdleMinutes;

        if (maxIdleMinutes <= 0 || _shellSettings.IsDefaultShell()) return;

        if (_lastActiveTimeAccessor.LastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= _clock?.UtcNow)
        {
            _logger?.LogWarning("Shutting down tenant \"{ShellName}\" because of idle timeout.", _shellSettings?.Name);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }
    }
}
