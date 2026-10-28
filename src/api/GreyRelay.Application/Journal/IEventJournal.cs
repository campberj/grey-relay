using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Journal;

/// <summary>
/// Append-only, per-campaign event journal: the authoritative record of committed outcomes
/// (ADR 0002). A campaign's revision is the sequence of its last event; zero when empty.
/// </summary>
public interface IEventJournal
{
    Task<long> GetRevisionAsync(CampaignId campaignId, CancellationToken cancellationToken);

    /// <summary>Reads events with sequence greater than <paramref name="afterSequence"/>, in order.</summary>
    Task<IReadOnlyList<RecordedEvent>> ReadAsync(
        CampaignId campaignId,
        long afterSequence,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically appends <paramref name="events"/> as sequences <c>expectedRevision + 1</c> onward.
    /// Throws <see cref="Domain.Common.RevisionConflictException"/> if the campaign has moved on.
    /// </summary>
    Task<IReadOnlyList<RecordedEvent>> AppendAsync(
        CampaignId campaignId,
        long expectedRevision,
        IReadOnlyList<NewEvent> events,
        CancellationToken cancellationToken);
}
