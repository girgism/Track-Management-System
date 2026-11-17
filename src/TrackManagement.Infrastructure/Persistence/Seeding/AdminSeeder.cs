using global::TrackManagement.Application.Common.Auth;
using global::TrackManagement.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

using TrackManagement.Infrastructure.Persistence.Auth;
using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence.Seeding;

internal static class AdminSeeder
{
    public static void Seed(DbContext context, AdminSeedOptions? admin, IPasswordHasher hasher)
    {
        if (admin is null)
            return;

        var db = (TrackManagementContext)context;

        if (db.Users.Any(u => u.Email == admin.Email))
            return;

        db.Users.Add(User.Create(admin.Email, hasher.Hash(admin.Password), UserRole.Admin));
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context,
                                       AdminSeedOptions? admin,
                                       IPasswordHasher hasher,
                                       CancellationToken cancellationToken)
    {
        if (admin is null)
            return;

        var db = (TrackManagementContext)context;

        if (await db.Users.AnyAsync(u => u.Email == admin.Email, cancellationToken))
            return;

        db.Users.Add(User.Create(admin.Email, hasher.Hash(admin.Password), UserRole.Admin));
        await db.SaveChangesAsync(cancellationToken);
    }
}