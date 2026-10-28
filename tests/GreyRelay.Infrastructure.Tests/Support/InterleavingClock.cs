using GreyRelay.Application.Abstractions;

namespace GreyRelay.Infrastructure.Tests.Support;

/// <summary>
/// The journal reads the clock after its revision check and before writing. This clock runs
/// <paramref name="interleave"/> on that first read, letting a test inject a competing write
/// into exactly that window.
/// </summary>
internal sealed class InterleavingClock(Action interleave) : IClock
{
    private Action? _pending = interleave;

    public DateTimeOffset UtcNow
    {
        get
        {
            var action = Interlocked.Exchange(ref _pending, null);
            action?.Invoke();
            return TestDatabase.RecordedAt;
        }
    }
}
