using Microsoft.EntityFrameworkCore;

using TrackManagement.Application.Dsps;
using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class DspRepository(TrackManagemenContext db) : IDspRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        await db.DSPs
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
}