using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Services;

public class StaggeredTenantWakeUpService : IStaggeredTenantWakeUpService
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IShellHost _shellHost;
    private readonly IContentManager _contentManager;
    private readonly ILogger<StaggeredTenantWakeUpService> _logger;
    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly IEnumerable<IStaggeredTenantWakeUpEvents> _staggeredTenantWakeUpEvents;
    private readonly ConcurrentDictionary<string, string> _versionUpdates = new();
    private readonly ConcurrentDictionary<string, string> _errorLogs = new();
    private readonly ConcurrentBag<string> _processedTenantIds = [];

    public StaggeredTenantWakeUpService(
        IShellHost shellHost,
        IContentManager contentManager,
        ILogger<StaggeredTenantWakeUpService> logger,
        ISession session,
        IClock clock,
        IEnumerable<IStaggeredTenantWakeUpEvents> staggeredTenantWakeUpEvents)
    {
        _shellHost = shellHost;
        _contentManager = contentManager;
        _logger = logger;
        _session = session;
        _clock = clock;
        _staggeredTenantWakeUpEvents = staggeredTenantWakeUpEvents;
    }

    public async Task<StaggeredTenantWakeUpPart> RunStaggeredTenantWakeUpAsync(bool newVersion = false)
    {
        // Only one thread can run the maintenance at a time, we don't want to run it in parallel. Also, we don't want
        // to run after each other, so we use a SemaphoreSlim to limit the number of concurrent threads to 1 and check
        // if the current count is 0 before running the maintenance.
        if (_lock.CurrentCount == 0)
        {
            return null;
        }

        await _lock.WaitAsync();

        var staggeredTenantWakeUpPart = (await GetOrCreateStaggeredTenantWakeUpSettingsAsync()).GetOrCreate<StaggeredTenantWakeUpPart>();
        await _staggeredTenantWakeUpEvents.AwaitEachAsync(async handler => await handler.StartingAsync(staggeredTenantWakeUpPart));
        try
        {
            await StaggeredTenantWakeUpAsync(staggeredTenantWakeUpPart, newVersion);
        }
        finally
        {
            staggeredTenantWakeUpPart.Finish(_clock);
            await SaveSettingsAsync(staggeredTenantWakeUpPart);

            await _staggeredTenantWakeUpEvents.AwaitEachAsync(async handler => await handler.FinishedAsync(staggeredTenantWakeUpPart));

            _lock.Release();
        }

        return staggeredTenantWakeUpPart;
    }

    public async Task<ContentItem> GetOrCreateStaggeredTenantWakeUpSettingsAsync()
    {
        var staggeredContentItem =
            await _session.Query<ContentItem, ContentItemIndex>(item => item.ContentType == ContentTypes.StaggeredTenantWakeUp)
                .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(staggeredContentItem?.ContentItemId))
        {
            staggeredContentItem = await _contentManager.NewAsync(ContentTypes.StaggeredTenantWakeUp);
            await _contentManager.CreateAsync(staggeredContentItem);
        }

        return staggeredContentItem;
    }

    /// <summary>
    /// Runs the staggered tenant wake-up for the remaining tenants. This is the main method that will be called to run
    /// the maintenance. It will start the maintenance, process the tenants in batches, and wait for the specified time
    /// between batches. It will also check if the maintenance was paused and if so, it will stop the maintenance. If
    /// the maintenance was paused, it will invoke the paused event for all registered handlers.
    /// </summary>
    private async Task StaggeredTenantWakeUpAsync(StaggeredTenantWakeUpPart staggeredTenantWakeUpPart, bool newVersion)
    {
        staggeredTenantWakeUpPart.Start(_clock, nameof(RunStaggeredTenantWakeUpAsync), newVersion);

        // Saving right after the start, so the UI can show the progress.
        await SaveSettingsAsync(staggeredTenantWakeUpPart);
        await _session.SaveChangesAsync();

        var remainingTenants = GetRemainingTenants(staggeredTenantWakeUpPart);

        while (remainingTenants.Count != 0)
        {
            if (staggeredTenantWakeUpPart.ShouldPause(nameof(RunStaggeredTenantWakeUpAsync)))
            {
                await InvokePausedEventAsync(staggeredTenantWakeUpPart);
                return;
            }

            await RunStaggeredTenantWakeUpForRemainingTenantsAsync(
                remainingTenants,
                staggeredTenantWakeUpPart);

            // Calculate percentage of completed tenants.
            staggeredTenantWakeUpPart.CalculatePercentage();

            await SaveSettingsAsync(staggeredTenantWakeUpPart);
            await _session.SaveChangesAsync();

            // Get the remaining tenants after processing, so if a new tenant is added it could be processed in the next
            // run.
            remainingTenants = GetRemainingTenants(staggeredTenantWakeUpPart);

            if (remainingTenants.Count != 0 && await WaitBeforeNextAsync(staggeredTenantWakeUpPart))
            {
                await InvokePausedEventAsync(staggeredTenantWakeUpPart);
                return;
            }
        }
    }

    /// <summary>
    /// Invokes the paused event for all registered handlers. This is to notify the handlers that the maintenance was
    /// paused.
    /// </summary>
    private Task InvokePausedEventAsync(StaggeredTenantWakeUpPart staggeredTenantWakeUpPart) =>
        _staggeredTenantWakeUpEvents.AwaitEachAsync(async handler => await handler.PausedAsync(staggeredTenantWakeUpPart));

    /// <summary>
    /// Waits for the specified time before processing the next tenant. This is to avoid overwhelming the system with
    /// requests and also checking periodically if the maintenance was paused.
    /// </summary>
    /// <returns><see langword="true"/> if the maintenance was paused, <see langword="false"/> otherwise.</returns>
    private async Task<bool> WaitBeforeNextAsync(StaggeredTenantWakeUpPart staggeredTenantWakeUpPart)
    {
        var waited = TimeSpan.Zero;
        var delay = TimeSpan.FromSeconds((int)staggeredTenantWakeUpPart.BatchIntervalSeconds.Value!);
        var delayCheckInterval = TimeSpan.FromMilliseconds(500);
        while (waited < delay)
        {
            if (staggeredTenantWakeUpPart.ShouldPause(nameof(RunStaggeredTenantWakeUpAsync)))
            {
                _logger.LogInformation("Maintenance paused during delay wait. Exiting.");
                return true;
            }

            var waitTime = delay - waited < delayCheckInterval ? delay - waited : delayCheckInterval;
            await Task.Delay(waitTime);
            waited += waitTime;
        }

        return false;
    }

    /// <summary>
    /// Gets the remaining tenants that need to be processed. This is done by getting all running tenants and excluding
    /// the ones that have already been processed. It also takes into account the number of tenants to be per batch, so
    /// it only returns the number of tenants that need to be processed in the current batch.
    /// </summary>
    private List<ShellSettings> GetRemainingTenants(StaggeredTenantWakeUpPart staggeredTenantWakeUpPart)
    {
        var allTenants = GetAllRunningTenantSettingsExceptDefault().ToList();

        var take = (int)staggeredTenantWakeUpPart.BatchSize.Value!;
        staggeredTenantWakeUpPart.AllTenantCount = allTenants.Count;

        return allTenants.Where(settings => !staggeredTenantWakeUpPart.ProcessedTenantIds.Contains(settings.TenantId))
            .Take(take)
            .ToList();
    }

    /// <summary>
    /// Gets all tenant settings which state is running, except the default tenant. This is done to avoid processing the
    /// default.
    /// </summary>
    private IEnumerable<ShellSettings> GetAllRunningTenantSettingsExceptDefault() =>
        _shellHost.GetAllSettings().Where(settings => !settings.IsDefaultShell() && settings.IsRunning());

    /// <summary>
    /// This is where the actual tenant wake-up is triggered for each processed tenant.
    /// </summary>
    private async Task RunStaggeredTenantWakeUpForRemainingTenantsAsync(
        List<ShellSettings> remainingTenants,
        StaggeredTenantWakeUpPart staggeredTenantWakeUpPart)
    {
        _versionUpdates.Clear();
        _errorLogs.Clear();
        _processedTenantIds.Clear();

        if (!staggeredTenantWakeUpPart.RunParallel.Value)
        {
            foreach (var remainingTenant in remainingTenants)
            {
                await WakeUpTenantAsync(remainingTenant, staggeredTenantWakeUpPart);
            }
        }
        else
        {
            var tasks = remainingTenants.Select(async remainingTenant =>
                await WakeUpTenantAsync(remainingTenant, staggeredTenantWakeUpPart));
            await Task.WhenAll(tasks);
        }

        staggeredTenantWakeUpPart.Versions.AddRangeWithOverwrite(_versionUpdates);
        staggeredTenantWakeUpPart.ErrorLogs.AddRange(_errorLogs);
        staggeredTenantWakeUpPart.ProcessedTenantIds.AddRange(_processedTenantIds);
    }

    private async Task WakeUpTenantAsync(
        ShellSettings tenant,
        StaggeredTenantWakeUpPart staggeredTenantWakeUpPart)
    {
        try
        {
            _shellHost.TryGetShellContext(tenant.Name, out var shellContext);

            if (!shellContext.IsActivated)
            {
                await _shellHost.WithShellScopeAsync(
                    scope =>
                    {
                        // Only logging is necessary here, as the actual maintenance and migration tasks are already
                        // done when we get here.
                        var tenantLogger = scope.ServiceProvider.GetRequiredService<ILogger<StaggeredTenantWakeUpService>>();
                        tenantLogger.LogInformation(
                            "Staggered tenant wake-up for current tenant finished successfully on version {Version}.",
                            staggeredTenantWakeUpPart.CurrentVersion);
                        return Task.CompletedTask;
                    },
                    tenant.Name);

                // We release the shell context to free up resources. Calling DisposeAsync() manually is needed before
                // this.
                await _shellHost.ReleaseShellContextAsync(tenant, eventSource: false);
            }

            _versionUpdates[tenant.Name] = staggeredTenantWakeUpPart.CurrentVersion.ToTechnicalString();

            _logger.LogInformation(
                "Staggered tenant wake-up for tenant '{TenantName}' finished successfully on version {Version}.",
                tenant.Name,
                staggeredTenantWakeUpPart.CurrentVersion);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Staggered tenant wake-up for tenant '{TenantName}' on version {Version} failed.",
                tenant.Name,
                staggeredTenantWakeUpPart.CurrentVersion);
            _errorLogs[tenant.Name] = $"{exception.Message} : {exception}";
        }
        finally
        {
            // We should always add the tenant to the processed list, even if it failed.
            _processedTenantIds.Add(tenant.TenantId);
        }
    }

    private Task SaveSettingsAsync(ContentPart part)
    {
        part.Apply();
        return _contentManager.CreateOrUpdateAsync(part.ContentItem);
    }
}
