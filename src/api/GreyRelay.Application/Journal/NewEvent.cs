using System.Text.Json;
using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Journal;

/// <summary>An event about to be appended; the journal assigns its sequence and recording time.</summary>
public sealed record NewEvent
{
    public required Guid EventId { get; init; }

    public required string EventType { get; init; }

    public required int EventSchemaVersion { get; init; }

    public required ActorRef Actor { get; init; }

    public required ActorRef RecordedBy { get; init; }

    public required EventAuthority Authority { get; init; }

    public required FictionalTime OccurredFictional { get; init; }

    public required JsonElement Payload { get; init; }

    public Guid? CausationId { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CorrectsEventId { get; init; }

    /// <summary>Pinned content-pack / rules-adapter versions, keyed by pack or adapter id.</summary>
    public IReadOnlyDictionary<string, string> ContentRefs { get; init; } = new Dictionary<string, string>();
}
