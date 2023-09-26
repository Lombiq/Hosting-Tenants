using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System;
using System.Threading;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

/// <summary>
/// Service to determine when the last request was initiated. If a request is sent the service will automatically update
/// the last active date time.
/// </summary>
public interface ILastActiveTimeAccessor
{
    /// <summary>
    /// Gets the last DateTime when a request was sent to the application.
    /// </summary>
    DateTime LastActiveDateTimeUtc { get; }

    /// <summary>
    /// Updates the LastActiveDateTimeUtc to the latest request's time.
    /// </summary>
    void Update(IClock clock);
}

public class LastActiveTimeAccessor : ILastActiveTimeAccessor
{
    private long _lastActiveDateTimeUtcTicks;

    public DateTime LastActiveDateTimeUtc
    {
        get => new(Interlocked.CompareExchange(ref _lastActiveDateTimeUtcTicks, 0, 0), DateTimeKind.Utc);
        private set => Interlocked.Exchange(ref _lastActiveDateTimeUtcTicks, value.Ticks);
    }

    public LastActiveTimeAccessor(IServiceProvider serviceProvider)
    {
        var clock = serviceProvider.GetService<IClock>();
        LastActiveDateTimeUtc = clock.UtcNow;
    }

    public void Update(IClock clock) => LastActiveDateTimeUtc = clock.UtcNow;
}
