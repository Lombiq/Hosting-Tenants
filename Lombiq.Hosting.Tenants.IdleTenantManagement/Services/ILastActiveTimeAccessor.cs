using OrchardCore.Modules;

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
    void Update();
}

public class LastActiveTimeAccessor : ILastActiveTimeAccessor
{
    private readonly IClock _clock;
    private long _lastActiveDateTimeUtcTicks;

    public DateTime LastActiveDateTimeUtc
    {
        get => new(Interlocked.CompareExchange(ref _lastActiveDateTimeUtcTicks, 0, 0));
        private set => Interlocked.Exchange(ref _lastActiveDateTimeUtcTicks, value.Ticks);
    }

    public LastActiveTimeAccessor(IClock clock)
    {
        _clock = clock;
        LastActiveDateTimeUtc = clock.UtcNow;
    }

    public void Update() => LastActiveDateTimeUtc = _clock.UtcNow;
}
