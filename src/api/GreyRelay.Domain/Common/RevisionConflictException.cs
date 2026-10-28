namespace GreyRelay.Domain.Common;

/// <summary>
/// Raised when a command was issued against a stale revision. Callers must reload and retry;
/// state is never silently overwritten (ARCHITECTURE section 4).
/// </summary>
public sealed class RevisionConflictException : DomainException
{
    public RevisionConflictException(long expectedRevision, long actualRevision)
        : base($"Expected revision {expectedRevision} but current revision is {actualRevision}.")
    {
        ExpectedRevision = expectedRevision;
        ActualRevision = actualRevision;
    }

    public long ExpectedRevision { get; }

    public long ActualRevision { get; }
}
