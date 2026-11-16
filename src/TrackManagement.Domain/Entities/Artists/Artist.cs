using System.Net.Mail;

using global::TrackManagement.Domain.Common;

namespace TrackManagement.Domain.Entities.Artists;

public sealed class Artist
{
    public const int NameMaxLength = 200;
    public const int EmailMaxLength = 254;
    public const int CountryCodeLength = 2;

    private Artist() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    /// <summary>ISO 3166-1 alpha-2 code, e.g. EG, SA, AE.</summary>
    public string Country { get; private set; } = null!;

    public static Artist Create(string? name, string? email, string? country)
    {
        return new Artist
        {
            Id = Guid.CreateVersion7(),
            Name = Guard.NotEmpty(name, nameof(name), NameMaxLength),
            Email = NormalizeEmail(email),
            Country = NormalizeCountry(country)
        };
    }

    private static string NormalizeEmail(string? email)
    {
        var value = Guard.NotEmpty(email, nameof(email), EmailMaxLength).ToLowerInvariant();

        if (!MailAddress.TryCreate(value, out var parsed) || parsed.Address != value)
            throw new DomainValidationException(nameof(email), "Email is not a valid address.");

        return value;
    }

    private static string NormalizeCountry(string? country)
    {
        var code = Guard.NotEmpty(country, nameof(country), CountryCodeLength).ToUpperInvariant();

        if (code.Length != CountryCodeLength || !code.All(char.IsAsciiLetterUpper))
            throw new DomainValidationException(nameof(country), "Country must be a 2-letter ISO code (e.g. EG).");

        return code;
    }
}
