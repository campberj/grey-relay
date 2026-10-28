namespace GreyRelay.Infrastructure.Persistence.Journal;

/// <summary>
/// Names shared between the journal mapping, migration InitialJournal and the adapter.
/// The trigger message must match the RAISE text in that migration.
/// </summary>
internal static class JournalSchema
{
    public const string TableName = "campaign_events";
    public const string ContiguousSequenceTrigger = "trg_campaign_events_contiguous_sequence";
    public const string PreventUpdateTrigger = "trg_campaign_events_prevent_update";
    public const string PreventDeleteTrigger = "trg_campaign_events_prevent_delete";
    public const string NonContiguousSequenceMessage = "campaign_events: sequence must follow the campaign's last event";
}
