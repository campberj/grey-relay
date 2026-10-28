using GreyRelay.Application.Abstractions;

namespace GreyRelay.Infrastructure.Tests.Support;

internal sealed class FixedClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}
