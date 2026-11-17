using System.Security.Cryptography;

using global::TrackManagement.Application.Common.Auth;

namespace TrackManagement.Infrastructure.Persistence.Auth;
/// <summary>
/// PBKDF2-HMAC-SHA256 with a per-password random salt. Stored as
/// "PBKDF2-SHA256$iterations$salt$hash" so parameters can be raised later without breaking old hashes.
/// </summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2-SHA256";
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    // Used when the user doesn't exist, so "unknown email" costs the same as "wrong password".
    private static readonly Lazy<string> DummyHash = new(() => Create("timing-equalization-password"));

    public string Hash(string password) => Create(password);

    public bool Verify(string? passwordHash, string password)
    {
        var parts = (passwordHash ?? DummyHash.Value).Split('$');

        if (parts.Length != 4
            || parts[0] != Prefix
            || !int.TryParse(parts[1], out var iterations)
            || iterations is < 1 or > 10_000_000)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        var matches = CryptographicOperations.FixedTimeEquals(actual, expected);

        // For the dummy hash the work above still happens, but the answer is always "no".
        return matches && passwordHash is not null;
    }

    private static string Create(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}