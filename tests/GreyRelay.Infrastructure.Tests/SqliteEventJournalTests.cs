using GreyRelay.Application.Abstractions;
using GreyRelay.Application.Journal;
using GreyRelay.Domain.Campaigns;
using GreyRelay.Domain.Common;
using GreyRelay.Infrastructure.Persistence;
using GreyRelay.Infrastructure.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Infrastructure.Tests;

public sealed class SqliteEventJournalTests : IAsyncLifetime
{
    private static readonly CampaignId CampaignA = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"));
    private static readonly CampaignId CampaignB = new(Guid.Parse("b0000000-0000-4000-8000-000000000002"));

    private TestDatabase _database = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _database = await TestDatabase.CreateAsync();

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task Empty_campaign_has_revision_zero_and_no_events()
    {
        await using var process = _database.StartProcess();
        var journal = Journal(process);

        Assert.Equal(0, await journal.GetRevisionAsync(CampaignA, Ct));
        Assert.Empty(await journal.ReadAsync(CampaignA, afterSequence: 0, Ct));
    }

    [Fact]
    public async Task Appended_events_get_contiguous_sequences_and_survive_restart()
    {
        var first = TestEvents.Create("first");
        var second = TestEvents.Create("second", FictionalTime.Unknown);

        await using (var process = _database.StartProcess())
        {
            var recorded = await Journal(process).AppendAsync(CampaignA, expectedRevision: 0, [first, second], Ct);
            Assert.Equal([1L, 2L], recorded.Select(e => e.Sequence));
        }

        await using (var restarted = _database.StartProcess())
        {
            var journal = Journal(restarted);
            var events = await journal.ReadAsync(CampaignA, afterSequence: 0, Ct);

            Assert.Equal(2, await journal.GetRevisionAsync(CampaignA, Ct));
            Assert.Equal([1L, 2L], events.Select(e => e.Sequence));
            Assert.All(events, e => Assert.Equal(CampaignA, e.CampaignId));
            Assert.All(events, e => Assert.Equal(TestDatabase.RecordedAt, e.RecordedAtUtc));
            TestEvents.AssertSameEnvelope(first, events[0].Event);
            TestEvents.AssertSameEnvelope(second, events[1].Event);
            Assert.False(events[1].Event.OccurredFictional.IsKnown);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task Append_against_wrong_revision_is_rejected_and_writes_nothing(long staleOrFutureRevision)
    {
        await using var process = _database.StartProcess();
        var journal = Journal(process);
        await journal.AppendAsync(CampaignA, 0, [TestEvents.Create("existing")], Ct);

        var conflict = await Assert.ThrowsAsync<RevisionConflictException>(
            () => journal.AppendAsync(CampaignA, staleOrFutureRevision, [TestEvents.Create("late")], Ct));

        Assert.Equal(staleOrFutureRevision, conflict.ExpectedRevision);
        Assert.Equal(1, conflict.ActualRevision);
        Assert.Equal(1, await journal.GetRevisionAsync(CampaignA, Ct));
    }

    [Fact]
    public async Task Second_writer_at_the_same_revision_gets_a_conflict()
    {
        await using var writerOne = _database.StartProcess();
        await using var writerTwo = _database.StartProcess();

        await Journal(writerOne).AppendAsync(CampaignA, 0, [TestEvents.Create("one")], Ct);

        await Assert.ThrowsAsync<RevisionConflictException>(
            () => Journal(writerTwo).AppendAsync(CampaignA, 0, [TestEvents.Create("two")], Ct));
    }

    [Fact]
    public async Task Race_between_revision_check_and_write_is_caught_by_the_database()
    {
        await using var competitor = _database.StartProcess();
        var services = new ServiceCollection();
        services.AddInfrastructure(_database.DatabasePath);
        services.AddSingleton<IClock>(new InterleavingClock(() =>
            Journal(competitor).AppendAsync(CampaignA, 0, [TestEvents.Create("competitor")], Ct).GetAwaiter().GetResult()));
        await using var racer = services.BuildServiceProvider();

        var conflict = await Assert.ThrowsAsync<RevisionConflictException>(
            () => Journal(racer).AppendAsync(CampaignA, 0, [TestEvents.Create("racer")], Ct));

        Assert.Equal(0, conflict.ExpectedRevision);
        Assert.Equal(1, conflict.ActualRevision);
        var survivors = await Journal(competitor).ReadAsync(CampaignA, 0, Ct);
        Assert.Equal("{\"note\":\"competitor\"}", survivors.Single().Event.Payload.GetRawText());
    }

    [Fact]
    public async Task A_failing_batch_writes_nothing()
    {
        await using var process = _database.StartProcess();
        var journal = Journal(process);
        var original = TestEvents.Create("original");
        var duplicateId = TestEvents.Create("duplicate") with { EventId = original.EventId };

        await Assert.ThrowsAsync<DbUpdateException>(
            () => journal.AppendAsync(CampaignA, 0, [original, duplicateId], Ct));

        await using var restarted = _database.StartProcess();
        Assert.Equal(0, await Journal(restarted).GetRevisionAsync(CampaignA, Ct));
    }

    [Fact]
    public async Task Campaigns_have_independent_sequences_and_reads()
    {
        await using var process = _database.StartProcess();
        var journal = Journal(process);

        await journal.AppendAsync(CampaignA, 0, [TestEvents.Create("a1"), TestEvents.Create("a2")], Ct);
        var b = await journal.AppendAsync(CampaignB, 0, [TestEvents.Create("b1")], Ct);

        Assert.Equal(1, b.Single().Sequence);
        Assert.Equal(2, await journal.GetRevisionAsync(CampaignA, Ct));
        Assert.Equal(1, await journal.GetRevisionAsync(CampaignB, Ct));
        Assert.All(await journal.ReadAsync(CampaignA, 0, Ct), e => Assert.Equal(CampaignA, e.CampaignId));
        Assert.All(await journal.ReadAsync(CampaignB, 0, Ct), e => Assert.Equal(CampaignB, e.CampaignId));
    }

    [Fact]
    public async Task Read_after_a_sequence_returns_only_later_events()
    {
        await using var process = _database.StartProcess();
        var journal = Journal(process);
        await journal.AppendAsync(CampaignA, 0, [TestEvents.Create("1"), TestEvents.Create("2"), TestEvents.Create("3")], Ct);

        var later = await journal.ReadAsync(CampaignA, afterSequence: 1, Ct);

        Assert.Equal([2L, 3L], later.Select(e => e.Sequence));
    }

    [Theory]
    [InlineData("UPDATE campaign_events SET event_type = 'tampered'")]
    [InlineData("DELETE FROM campaign_events")]
    public async Task Database_rejects_editing_or_deleting_history(string sql)
    {
        await using var process = _database.StartProcess();
        await Journal(process).AppendAsync(CampaignA, 0, [TestEvents.Create("history")], Ct);

        var error = await Assert.ThrowsAsync<SqliteException>(() => Database(process).ExecuteSqlRawAsync(sql, Ct));

        Assert.Contains("append-only", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, await Journal(process).GetRevisionAsync(CampaignA, Ct));
    }

    [Fact]
    public async Task Database_rejects_an_insert_that_skips_a_sequence_but_accepts_the_next_one()
    {
        await using var process = _database.StartProcess();
        await Journal(process).AppendAsync(CampaignA, 0, [TestEvents.Create("first")], Ct);

        var gap = await Assert.ThrowsAsync<SqliteException>(() => CopyFirstEventAs(process, sequence: 3));
        await CopyFirstEventAs(process, sequence: 2);

        Assert.Contains("sequence must follow", gap.Message, StringComparison.Ordinal);
        Assert.Equal(2, await Journal(process).GetRevisionAsync(CampaignA, Ct));
    }

    private static Task<int> CopyFirstEventAs(IServiceProvider process, long sequence) =>
        Database(process).ExecuteSqlAsync(
            $"""
            INSERT INTO campaign_events
            SELECT campaign_id, {sequence}, lower(hex(randomblob(16))), event_type, event_schema_version,
                   actor_kind, actor_id, recorded_by_kind, recorded_by_id, authority_kind, authority_reference,
                   causation_id, correlation_id, corrects_event_id, occurred_known, occurred_day, occurred_time_z,
                   occurred_precision, recorded_at_utc, content_refs, payload
            FROM campaign_events WHERE sequence = 1
            """,
            Ct);

    private static IEventJournal Journal(IServiceProvider process) =>
        process.CreateScope().ServiceProvider.GetRequiredService<IEventJournal>();

    private static Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database(IServiceProvider process) =>
        process.CreateScope().ServiceProvider.GetRequiredService<GreyRelayDbContext>().Database;
}
