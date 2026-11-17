
using global::TrackManagement.Domain.Common;

namespace TrackManagement.Domain.Entities.Users;

public sealed class User
{
    public const int EmailMaxLength = 254;
    public const int PasswordHashMaxLength = 256;

    private User() { }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public static User Create(string? email, string? passwordHash, UserRole role)
    {
        if (!Enum.IsDefined(role))
            throw new DomainValidationException(nameof(role), "Role is not a valid value.");

        return new User
        {
            Id = Guid.CreateVersion7(),
            Email = Guard.NotEmpty(email, nameof(email), EmailMaxLength).ToLowerInvariant(),
            PasswordHash = Guard.NotEmpty(passwordHash, nameof(passwordHash), PasswordHashMaxLength),
            Role = role
        };
    }
}
