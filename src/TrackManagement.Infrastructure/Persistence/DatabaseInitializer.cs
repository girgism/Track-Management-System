using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations. The configured UseSeeding/UseAsyncSeeding hooks run as part of
    /// MigrateAsync, so sample data and the admin user are created at the same time.
    /// </summary>
    public static async Task ApplyMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<TrackManagementContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }
}
