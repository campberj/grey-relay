namespace GreyRelay.Application.Journal;

/// <summary>Identifies who decided (actor) or who entered (recorded_by) an event.</summary>
public sealed record ActorRef
{
    public ActorRef(ActorKind kind, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Kind = kind;
        Id = id;
    }

    public ActorKind Kind { get; }

    public string Id { get; }
}
