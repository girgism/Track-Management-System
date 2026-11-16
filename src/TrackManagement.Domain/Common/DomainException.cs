namespace TrackManagement.Domain.Common;

/// <summary>Base type for all business-rule violations. Code is a stable, machine-readable identifier.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

/// <summary>An invariant on a single field was violated (empty name, malformed ISRC, ...).</summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string field, string message)
        : base("domain.validation", message)
    {
        Field = field;
    }

    public string Field { get; }
}