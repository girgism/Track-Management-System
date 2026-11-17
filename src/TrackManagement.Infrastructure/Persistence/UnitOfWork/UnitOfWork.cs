using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence;

/// <summary>
/// Commits the DbContext and translates database-level failures into Application exceptions,
/// so no SQL or EF type ever reaches the use cases or the API.
/// </summary>
internal sealed class UnitOfWork(TrackManagementContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Row-version check failed: another request changed the same row first.
            throw new ConflictException(
                "concurrency.conflict",
                "The resource was modified by another request. Reload it and try again.",
                ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sql)
        {
            // Unique index / constraint violation: the race that the pre-checks cannot fully close.
            throw ToConflict(sql, ex);
        }
    }

    private static ConflictException ToConflict(SqlException sql, Exception inner)
    {
        var message = sql.Message;

        if (message.Contains("UX_Tracks_Isrc", StringComparison.Ordinal))
            return new("track.duplicate_isrc", "A track with this ISRC already exists.", inner);

        if (message.Contains("UX_TrackDistributions_TrackId_DspId", StringComparison.Ordinal))
            return new("track.duplicate_distribution",
                "The track was just submitted to one of these DSPs by another request.", inner);

        if (message.Contains("UX_Artists_Email", StringComparison.Ordinal))
            return new("artist.duplicate_email", "An artist with this email already exists.", inner);

        return new("conflict.duplicate", "The request conflicts with an existing resource.", inner);
    }
}