using global::TrackManagement.Domain.Entities.Users;

namespace TrackManagement.Application.Common.Auth;

public sealed record IssuedToken(string Value, DateTimeOffset ExpiresAtUtc);

public interface ITokenService
{
    IssuedToken Generate(User user);
}
