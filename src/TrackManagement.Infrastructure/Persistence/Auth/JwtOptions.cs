using System.Text;

using Microsoft.Extensions.Configuration;
namespace TrackManagement.Infrastructure.Persistence.Auth;

/// <summary>JWT settings shared by the issuer (Infrastructure) and the validator (Api).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    private const int MinimumKeyBytes = 32; // HS256 needs at least a 256-bit key

    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string SigningKey { get; init; } = "";
    public int ExpiryMinutes { get; init; } = 60;

    /// <summary>Reads and validates the settings. Throws at startup, so a bad config can never reach production silently.</summary>
    public static JwtOptions Load(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        var options = new JwtOptions
        {
            Issuer = section["Issuer"] ?? "",
            Audience = section["Audience"] ?? "",
            SigningKey = section["SigningKey"] ?? "",
            ExpiryMinutes = int.TryParse(section["ExpiryMinutes"], out var minutes) ? minutes : 60
        };

        options.Validate();
        return options;
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Jwt:Issuer is not configured.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Audience is not configured.");

        if (Encoding.UTF8.GetByteCount(SigningKey) < MinimumKeyBytes)
            throw new InvalidOperationException(
                $"Jwt:SigningKey is missing or shorter than {MinimumKeyBytes} bytes. " +
                "Provide it via user secrets or the Jwt__SigningKey environment variable.");

        if (ExpiryMinutes is < 1 or > 1440)
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be between 1 and 1440.");
    }
}