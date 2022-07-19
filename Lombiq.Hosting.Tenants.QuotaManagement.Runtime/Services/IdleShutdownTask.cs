using NLog;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;

public class IdleShutdownTask : IBackgroundTask
{
    private readonly IClock _clock;
    private readonly ShellSettings _shellSettings;
    private readonly ILastActiveTimeAccessor _lastActiveTimeAccessor;
    private readonly IShellSettingsManager _shellSettingsManager;
    private readonly IShellHost _shellHost;
    
    public ILogger Logger { get; set; }

    public IdleShutdownTask(
        IClock clock,
        ShellSettings shellSettings,
        ILastActiveTimeAccessor lastActiveTimeAccessor,
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost)
    {
        _clock = clock;
        _shellSettings = shellSettings;
        _lastActiveTimeAccessor = lastActiveTimeAccessor;
        _shellSettingsManager = shellSettingsManager;
        _shellHost = shellHost;

        Logger = Logger.Factory.CreateNullLogger();
    }

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var maxIdleMinutes = 9999999;

        if (maxIdleMinutes <= 0) return Task.CompletedTask;

        if (_lastActiveTimeAccessor.LastActiveDateTimeUtc.AddMinutes(maxIdleMinutes) <= _clock.UtcNow)
        {
            Logger.Info($"Shutting down tenant \"{_shellSettings.Name}\" because of idle timeout.");

            try
            {
                InvokeShutdown();
            }
            catch (Exception e)
            {
                Logger.Error(
                    e,
                    $"Shutting down \"{_shellSettings.Name}\" because of idle timeout failed with the following exception. Another shutdown will be attempted.");

                // If the ShellContext.Dispose() fails (which can happen with a DB error: then the transaction 
                // commits triggered by the dispose will fail) then while the tenant is unavailable the shell is 
                // still active in a failed state. So first we need to correctly start the tenant, then shut it
                // down for good.

                _shellSettings.State = TenantState.Running;
                InvokeRestart();
                InvokeShutdown();
            }
        }
        
        return Task.CompletedTask;
    }
    
    private void InvokeShutdown()
    {
        _shellSettings.State = TenantState.Disabled;
        InvokeRestart();
    }
    
    private void InvokeRestart()
    {
        _shellSettingsManager.SaveSettingsAsync(_shellSettings);
        _shellHost.UpdateShellSettingsAsync(_shellSettings);
    }
}