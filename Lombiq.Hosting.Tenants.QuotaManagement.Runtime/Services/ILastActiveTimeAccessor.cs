using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;

public interface ILastActiveTimeAccessor
{
    DateTime LastActiveDateTimeUtc { get; }
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
