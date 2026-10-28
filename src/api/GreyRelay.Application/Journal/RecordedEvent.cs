using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Journal;

/// <summary>An event as committed to a campaign's journal.</summary>
public sealed record RecordedEvent(
    CampaignId CampaignId,
    long Sequence,
    DateTimeOffset RecordedAtUtc,
    NewEvent Event);
