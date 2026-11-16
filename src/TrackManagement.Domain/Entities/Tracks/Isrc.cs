using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using global::TrackManagement.Domain.Common;
namespace TrackManagement.Domain.Entities.Tracks;

/// <summary>
/// International Standard Recording Code. Stored in canonical form: 12 chars, uppercase, no separators.
/// Format: 2-letter country + 3 alphanumeric registrant + 2-digit year + 5-digit designation.
/// </summary>
public sealed record Isrc
{
    public const int Length = 12;

    private static readonly Regex Format =
        new("^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Isrc(string value) => Value = value;

    public string Value { get; }

    public static Isrc Create(string? input)
    {
        if (!TryCreate(input, out var isrc))
            throw new DomainValidationException(
                "isrc",
                "ISRC must be 12 characters: 2-letter country, 3-character registrant, 2-digit year, 5-digit designation (e.g. EG-A01-23-00001).");

        return isrc;
    }

    public static bool TryCreate(string? input, [NotNullWhen(true)] out Isrc? isrc)
    {
        isrc = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var normalized = input.Replace("-", "").Replace(" ", "").ToUpperInvariant();

        if (!Format.IsMatch(normalized))
            return false;

        isrc = new Isrc(normalized);
        return true;
    }

    public override string ToString() => Value;
}
