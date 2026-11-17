using Microsoft.Extensions.Configuration;
/// <summary>Credentials for the seeded admin. Absent configuration means no admin is seeded.</summary>
namespace TrackManagement.Infrastructure.Persistence.Auth;

internal sealed record AdminSeedOptions(string Email, string Password)
{
    private const int MinimumPasswordLength = 10;

    public static AdminSeedOptions? Load(IConfiguration configuration)
    {
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        if (password.Length < MinimumPasswordLength)
            throw new InvalidOperationException(
                $"Seed:AdminPassword must be at least {MinimumPasswordLength} characters.");

        return new AdminSeedOptions(email.Trim().ToLowerInvariant(), password);
    }
}