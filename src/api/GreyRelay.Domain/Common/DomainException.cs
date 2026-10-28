namespace GreyRelay.Domain.Common;

/// <summary>Base type for violations of a domain rule or invariant.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
