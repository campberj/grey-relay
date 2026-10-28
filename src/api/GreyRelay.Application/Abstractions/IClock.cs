namespace GreyRelay.Application.Abstractions;

/// <summary>Real-world time source; injected so recorded behaviour is replayable in tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
