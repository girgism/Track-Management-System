namespace TrackManagement.Application.Features.Users;

using global::TrackManagement.Domain.Entities.Users;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
}
