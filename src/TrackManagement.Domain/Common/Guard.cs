namespace TrackManagement.Domain.Common;

internal static class Guard
{
    /// <summary>Requires a non-blank string within maxLength; returns it trimmed.</summary>
    public static string NotEmpty(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException(field, $"{field} is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainValidationException(field, $"{field} must not exceed {maxLength} characters.");

        return trimmed;
    }

    public static Guid NotEmpty(Guid value, string field)
    {
        if (value == Guid.Empty)
            throw new DomainValidationException(field, $"{field} is required.");

        return value;
    }
}
