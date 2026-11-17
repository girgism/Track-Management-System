using global::TrackManagement.Application.Features.Users;
using global::TrackManagement.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(TrackManagementContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {

        return db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}
