using GreyRelay.Application.Abstractions;
using GreyRelay.Application.Journal;
using GreyRelay.Domain.Campaigns;
using GreyRelay.Domain.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GreyRelay.Infrastructure.Persistence.Journal;

/// <summary>
/// SQLite adapter for <see cref="IEventJournal"/>. The revision check here gives a clear error;
/// the database triggers and primary key are the backstop if two writers race.
/// </summary>
internal sealed class SqliteEventJournal(GreyRelayDbContext dbContext, IClock clock) : IEventJournal
{
    public async Task<long> GetRevisionAsync(CampaignId campaignId, CancellationToken cancellationToken) =>
        await dbContext.CampaignEvents
            .Where(e => e.CampaignId == campaignId.Value)
            .MaxAsync(e => (long?)e.Sequence, cancellationToken)
            .ConfigureAwait(false) ?? 0;

    public async Task<IReadOnlyList<RecordedEvent>> ReadAsync(
        CampaignId campaignId,
        long afterSequence,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.CampaignEvents
            .AsNoTracking()
            .Where(e => e.CampaignId == campaignId.Value && e.Sequence > afterSequence)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return records.Select(CampaignEventMapper.ToRecordedEvent).ToList();
    }

    public async Task<IReadOnlyList<RecordedEvent>> AppendAsync(
        CampaignId campaignId,
        long expectedRevision,
        IReadOnlyList<NewEvent> events,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        if (events.Count == 0)
        {
            throw new ArgumentException("At least one event is required.", nameof(events));
        }

        var actualRevision = await GetRevisionAsync(campaignId, cancellationToken).ConfigureAwait(false);
        if (actualRevision != expectedRevision)
        {
            throw new RevisionConflictException(expectedRevision, actualRevision);
        }

        var recordedAt = clock.UtcNow;
        var recorded = events
            .Select((e, index) => new RecordedEvent(campaignId, expectedRevision + 1 + index, recordedAt, e))
            .ToList();
        var records = recorded.Select(CampaignEventMapper.ToRecord).ToList();
        dbContext.CampaignEvents.AddRange(records);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception)
        {
            Detach(records);
            if (IsSequenceConflict(exception))
            {
                var currentRevision = await GetRevisionAsync(campaignId, cancellationToken).ConfigureAwait(false);
                throw new RevisionConflictException(expectedRevision, currentRevision);
            }

            throw;
        }

        return recorded;
    }

    private void Detach(IEnumerable<CampaignEventRecord> records)
    {
        foreach (var record in records)
        {
            dbContext.Entry(record).State = EntityState.Detached;
        }
    }

    private static bool IsSequenceConflict(DbUpdateException exception) =>
        exception.InnerException is SqliteException sqlite
        && (sqlite.SqliteExtendedErrorCode == SqliteErrorCodes.PrimaryKeyConstraint
            || sqlite.Message.Contains(JournalSchema.NonContiguousSequenceMessage, StringComparison.Ordinal));

    private static class SqliteErrorCodes
    {
        public const int PrimaryKeyConstraint = 1555;
    }
}
