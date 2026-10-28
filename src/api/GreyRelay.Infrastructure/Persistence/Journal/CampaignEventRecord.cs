namespace GreyRelay.Infrastructure.Persistence.Journal;

/// <summary>Row shape of <c>campaign_events</c>. Persistence-only; mapped by <see cref="CampaignEventMapper"/>.</summary>
internal sealed class CampaignEventRecord
{
    public required Guid CampaignId { get; init; }

    public required long Sequence { get; init; }

    public required Guid EventId { get; init; }

    public required string EventType { get; init; }

    public required int EventSchemaVersion { get; init; }

    public required string ActorKind { get; init; }

    public required string ActorId { get; init; }

    public required string RecordedByKind { get; init; }

    public required string RecordedById { get; init; }

    public required string AuthorityKind { get; init; }

    public string? AuthorityReference { get; init; }

    public Guid? CausationId { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CorrectsEventId { get; init; }

    public required bool OccurredKnown { get; init; }

    public int? OccurredDay { get; init; }

    public string? OccurredTimeZ { get; init; }

    public string? OccurredPrecision { get; init; }

    public required DateTimeOffset RecordedAtUtc { get; init; }

    public required string ContentRefs { get; init; }

    public required string Payload { get; init; }
}
