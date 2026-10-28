using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreyRelay.Infrastructure.Persistence.Journal;

internal sealed class CampaignEventRecordConfiguration : IEntityTypeConfiguration<CampaignEventRecord>
{
    public void Configure(EntityTypeBuilder<CampaignEventRecord> builder)
    {
        builder.ToTable(JournalSchema.TableName, table =>
        {
            table.HasCheckConstraint("ck_campaign_events_sequence_positive", "sequence >= 1");
            table.HasCheckConstraint("ck_campaign_events_payload_json", "json_valid(payload)");
            table.HasCheckConstraint("ck_campaign_events_content_refs_json", "json_valid(content_refs)");
            table.HasCheckConstraint(
                "ck_campaign_events_actor_kind",
                "actor_kind IN ('player','gm','system','import') AND recorded_by_kind IN ('player','gm','system','import')");
            table.HasCheckConstraint(
                "ck_campaign_events_occurred",
                "(occurred_known = 1 AND occurred_day IS NOT NULL AND occurred_day >= 0 AND occurred_time_z IS NOT NULL)"
                + " OR (occurred_known = 0 AND occurred_day IS NULL AND occurred_time_z IS NULL AND occurred_precision IS NULL)");

            // Created in migration InitialJournal; declared so EF does not rely on RETURNING around them.
            table.HasTrigger(JournalSchema.ContiguousSequenceTrigger);
            table.HasTrigger(JournalSchema.PreventUpdateTrigger);
            table.HasTrigger(JournalSchema.PreventDeleteTrigger);
        });

        builder.HasKey(e => new { e.CampaignId, e.Sequence });
        builder.HasIndex(e => e.EventId).IsUnique();

        builder.Property(e => e.CampaignId).HasColumnName("campaign_id");
        builder.Property(e => e.Sequence).HasColumnName("sequence").ValueGeneratedNever();
        builder.Property(e => e.EventId).HasColumnName("event_id");
        builder.Property(e => e.EventType).HasColumnName("event_type");
        builder.Property(e => e.EventSchemaVersion).HasColumnName("event_schema_version");
        builder.Property(e => e.ActorKind).HasColumnName("actor_kind");
        builder.Property(e => e.ActorId).HasColumnName("actor_id");
        builder.Property(e => e.RecordedByKind).HasColumnName("recorded_by_kind");
        builder.Property(e => e.RecordedById).HasColumnName("recorded_by_id");
        builder.Property(e => e.AuthorityKind).HasColumnName("authority_kind");
        builder.Property(e => e.AuthorityReference).HasColumnName("authority_reference");
        builder.Property(e => e.CausationId).HasColumnName("causation_id");
        builder.Property(e => e.CorrelationId).HasColumnName("correlation_id");
        builder.Property(e => e.CorrectsEventId).HasColumnName("corrects_event_id");
        builder.Property(e => e.OccurredKnown).HasColumnName("occurred_known");
        builder.Property(e => e.OccurredDay).HasColumnName("occurred_day");
        builder.Property(e => e.OccurredTimeZ).HasColumnName("occurred_time_z");
        builder.Property(e => e.OccurredPrecision).HasColumnName("occurred_precision");
        builder.Property(e => e.RecordedAtUtc).HasColumnName("recorded_at_utc");
        builder.Property(e => e.ContentRefs).HasColumnName("content_refs");
        builder.Property(e => e.Payload).HasColumnName("payload");
    }
}
