using Lombiq.Hosting.Tenants.IdleTenantManagement.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

[BackgroundTask(Schedule = "* * * * *", Description = "Disable Idle Tenants.")]
public class IdleShutdownTask : IBackgroundTask
{
    private readonly IClock _clock;
    private readonly ShellSettings _shellSettings;
    private readonly ILastActiveTimeAccessor _lastActiveTimeAccessor;
    private readonly IShellSettingsManager _shellSettingsManager;
    private readonly IShellHost _shellHost;
    private readonly ILogger<IdleShutdownTask> _logger;

    public IdleShutdownTask(
        IClock clock,
        ShellSettings shellSettings,
        ILastActiveTimeAccessor lastActiveTimeAccessor,
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        ILogger<IdleShutdownTask> logger)
    {
        _clock = clock;
        _shellSettings = shellSettings;
        _lastActiveTimeAccessor = lastActiveTimeAccessor;
        _shellSettingsManager = shellSettingsManager;
        _shellHost = shellHost;
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var maxIdleMinutes = _shellSettings.RuntimeQuotaSettings().MaxIdleMinutes;

        if (maxIdleMinutes <= 0) return;

        if (_lastActiveTimeAccessor.LastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= _clock.UtcNow)
        {
            _logger.LogInformation("Shutting down tenant \"{ShellName}\" because of idle timeout", _shellSettings.Name);

            try
            {
                await InvokeShutdownAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Shutting down \"{ShellName}\" because of idle timeout failed with the following exception. Another shutdown will be attempted",
                    _shellSettings.Name);

                // If the ShellContext.Dispose() fails (which can happen with a DB error: then the transaction
                // commits triggered by the dispose will fail) then while the tenant is unavailable the shell is
                // still active in a failed state. So first we need to correctly start the tenant, then shut it
                // down for good.

                _shellSettings.State = TenantState.Running;
                await InvokeRestartAsync();
                await InvokeShutdownAsync();
            }
        }
    }

    private Task InvokeShutdownAsync()
    {
        _shellSettings.State = TenantState.Disabled;
        return _shellHost.ReleaseShellContextAsync(_shellSettings);
    }

    private async Task InvokeRestartAsync()
    {
        await _shellSettingsManager.SaveSettingsAsync(_shellSettings);
        await _shellHost.UpdateShellSettingsAsync(_shellSettings);
    }
}
