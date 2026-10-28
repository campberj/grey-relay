using System.Text.Json;
using GreyRelay.Application.Journal;
using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Infrastructure.Persistence.Journal;

internal static class CampaignEventMapper
{
    public static CampaignEventRecord ToRecord(RecordedEvent recorded)
    {
        var e = recorded.Event;
        return new CampaignEventRecord
        {
            CampaignId = recorded.CampaignId.Value,
            Sequence = recorded.Sequence,
            EventId = e.EventId,
            EventType = e.EventType,
            EventSchemaVersion = e.EventSchemaVersion,
            ActorKind = ToStorage(e.Actor.Kind),
            ActorId = e.Actor.Id,
            RecordedByKind = ToStorage(e.RecordedBy.Kind),
            RecordedById = e.RecordedBy.Id,
            AuthorityKind = e.Authority.Kind,
            AuthorityReference = e.Authority.Reference,
            CausationId = e.CausationId,
            CorrelationId = e.CorrelationId,
            CorrectsEventId = e.CorrectsEventId,
            OccurredKnown = e.OccurredFictional.IsKnown,
            OccurredDay = e.OccurredFictional.Day,
            OccurredTimeZ = e.OccurredFictional.TimeZ,
            OccurredPrecision = e.OccurredFictional.Precision,
            RecordedAtUtc = recorded.RecordedAtUtc,
            ContentRefs = JsonSerializer.Serialize(e.ContentRefs),
            Payload = e.Payload.GetRawText(),
        };
    }

    public static RecordedEvent ToRecordedEvent(CampaignEventRecord record) =>
        new(
            new CampaignId(record.CampaignId),
            record.Sequence,
            record.RecordedAtUtc,
            new NewEvent
            {
                EventId = record.EventId,
                EventType = record.EventType,
                EventSchemaVersion = record.EventSchemaVersion,
                Actor = new ActorRef(FromStorage(record.ActorKind), record.ActorId),
                RecordedBy = new ActorRef(FromStorage(record.RecordedByKind), record.RecordedById),
                Authority = new EventAuthority(record.AuthorityKind, record.AuthorityReference),
                CausationId = record.CausationId,
                CorrelationId = record.CorrelationId,
                CorrectsEventId = record.CorrectsEventId,
                OccurredFictional = record.OccurredKnown
                    ? FictionalTime.At(record.OccurredDay!.Value, record.OccurredTimeZ!, record.OccurredPrecision)
                    : FictionalTime.Unknown,
                ContentRefs = JsonSerializer.Deserialize<Dictionary<string, string>>(record.ContentRefs) ?? [],
                Payload = ParseJson(record.Payload),
            });

    private static JsonElement ParseJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string ToStorage(ActorKind kind) => kind switch
    {
        ActorKind.Player => "player",
        ActorKind.Gm => "gm",
        ActorKind.System => "system",
        ActorKind.Import => "import",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown actor kind."),
    };

    private static ActorKind FromStorage(string kind) => kind switch
    {
        "player" => ActorKind.Player,
        "gm" => ActorKind.Gm,
        "system" => ActorKind.System,
        "import" => ActorKind.Import,
        _ => throw new InvalidOperationException($"Stored actor kind '{kind}' is not recognised."),
    };
}
