namespace GreyRelay.Application.Journal;

/// <summary>
/// What authorised an event, e.g. kind "gm_ruling" with a reference citing the chat ruling.
/// Kinds are governed by the event catalogue (ADR 0002 section 2.6).
/// </summary>
public sealed record EventAuthority
{
    public EventAuthority(string kind, string? reference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        Kind = kind;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference;
    }

    public string Kind { get; }

    public string? Reference { get; }
}
