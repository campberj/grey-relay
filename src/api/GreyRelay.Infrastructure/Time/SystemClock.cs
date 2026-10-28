using GreyRelay.Application.Abstractions;

namespace GreyRelay.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
