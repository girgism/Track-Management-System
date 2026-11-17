using Microsoft.EntityFrameworkCore;

using TrackManagement.Application.Features.Artists;
using TrackManagement.Domain.Entities.Artists;
using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class ArtistRepository(TrackManagementContext db) : IArtistRepository
{
    public void Add(Artist artist) => db.Artists.Add(artist);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Artists.AnyAsync(a => a.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Artists.AnyAsync(a => a.Email == normalizedEmail, cancellationToken);

    public async Task<IReadOnlyList<ArtistResponse>> ListAsync(CancellationToken cancellationToken) =>
        await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ArtistResponse(a.Id, a.Name, a.Email, a.Country))
            .ToListAsync(cancellationToken);
}