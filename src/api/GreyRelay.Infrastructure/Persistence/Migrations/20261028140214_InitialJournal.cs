using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreyRelay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaign_events",
                columns: table => new
                {
                    campaign_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sequence = table.Column<long>(type: "INTEGER", nullable: false),
                    event_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    event_type = table.Column<string>(type: "TEXT", nullable: false),
                    event_schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                    actor_kind = table.Column<string>(type: "TEXT", nullable: false),
                    actor_id = table.Column<string>(type: "TEXT", nullable: false),
                    recorded_by_kind = table.Column<string>(type: "TEXT", nullable: false),
                    recorded_by_id = table.Column<string>(type: "TEXT", nullable: false),
                    authority_kind = table.Column<string>(type: "TEXT", nullable: false),
                    authority_reference = table.Column<string>(type: "TEXT", nullable: true),
                    causation_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    correlation_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    corrects_event_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    occurred_known = table.Column<bool>(type: "INTEGER", nullable: false),
                    occurred_day = table.Column<int>(type: "INTEGER", nullable: true),
                    occurred_time_z = table.Column<string>(type: "TEXT", nullable: true),
                    occurred_precision = table.Column<string>(type: "TEXT", nullable: true),
                    recorded_at_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    content_refs = table.Column<string>(type: "TEXT", nullable: false),
                    payload = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaign_events", x => new { x.campaign_id, x.sequence });
                    table.CheckConstraint("ck_campaign_events_actor_kind", "actor_kind IN ('player','gm','system','import') AND recorded_by_kind IN ('player','gm','system','import')");
                    table.CheckConstraint("ck_campaign_events_content_refs_json", "json_valid(content_refs)");
                    table.CheckConstraint("ck_campaign_events_occurred", "(occurred_known = 1 AND occurred_day IS NOT NULL AND occurred_day >= 0 AND occurred_time_z IS NOT NULL) OR (occurred_known = 0 AND occurred_day IS NULL AND occurred_time_z IS NULL AND occurred_precision IS NULL)");
                    table.CheckConstraint("ck_campaign_events_payload_json", "json_valid(payload)");
                    table.CheckConstraint("ck_campaign_events_sequence_positive", "sequence >= 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_campaign_events_event_id",
                table: "campaign_events",
                column: "event_id",
                unique: true);

            // Hand-written: the journal is append-only and gap-free per campaign (ADR 0002 sections 2.3-2.4).
            // The RAISE text must match JournalSchema.NonContiguousSequenceMessage.
            // Any future migration that rebuilds this table must recreate these triggers.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_campaign_events_contiguous_sequence
                BEFORE INSERT ON campaign_events
                FOR EACH ROW
                WHEN NEW.sequence <> COALESCE(
                    (SELECT MAX(sequence) FROM campaign_events WHERE campaign_id = NEW.campaign_id), 0) + 1
                BEGIN
                    SELECT RAISE(ABORT, 'campaign_events: sequence must follow the campaign''s last event');
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_campaign_events_prevent_update
                BEFORE UPDATE ON campaign_events
                BEGIN
                    SELECT RAISE(ABORT, 'campaign_events is append-only; record an event.corrected event instead');
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_campaign_events_prevent_delete
                BEFORE DELETE ON campaign_events
                BEGIN
                    SELECT RAISE(ABORT, 'campaign_events is append-only; record an event.corrected event instead');
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_events");
        }
    }
}
