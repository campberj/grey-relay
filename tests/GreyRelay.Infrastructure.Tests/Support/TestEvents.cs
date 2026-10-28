using System.Text.Json;
using GreyRelay.Application.Journal;
using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Infrastructure.Tests.Support;

internal static class TestEvents
{
    public static NewEvent Create(string note, FictionalTime? occurred = null) => new()
    {
        EventId = Guid.NewGuid(),
        EventType = "test.noted",
        EventSchemaVersion = 1,
        Actor = new ActorRef(ActorKind.Gm, "overwatch"),
        RecordedBy = new ActorRef(ActorKind.Player, "owner"),
        Authority = new EventAuthority("gm_ruling", "Chat ruling, 2026-10-28"),
        OccurredFictional = occurred ?? FictionalTime.At(2, "18:45", "narrative"),
        Payload = JsonSerializer.SerializeToElement(new { note }),
        CausationId = Guid.NewGuid(),
        ContentRefs = new Dictionary<string, string> { ["ash-veil"] = "1.0.0" },
    };

    public static void AssertSameEnvelope(NewEvent expected, NewEvent actual)
    {
        Assert.Equal(expected.EventId, actual.EventId);
        Assert.Equal(expected.EventType, actual.EventType);
        Assert.Equal(expected.EventSchemaVersion, actual.EventSchemaVersion);
        Assert.Equal(expected.Actor, actual.Actor);
        Assert.Equal(expected.RecordedBy, actual.RecordedBy);
        Assert.Equal(expected.Authority, actual.Authority);
        Assert.Equal(expected.OccurredFictional, actual.OccurredFictional);
        Assert.Equal(expected.CausationId, actual.CausationId);
        Assert.Equal(expected.CorrelationId, actual.CorrelationId);
        Assert.Equal(expected.CorrectsEventId, actual.CorrectsEventId);
        Assert.Equal(expected.ContentRefs, actual.ContentRefs);
        Assert.Equal(expected.Payload.GetRawText(), actual.Payload.GetRawText());
    }
}
