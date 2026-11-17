using Microsoft.EntityFrameworkCore;

using TrackManagement.Application.Artists;
using TrackManagement.Application.Common.Models;
using TrackManagement.Application.Tracks;
using TrackManagement.Domain.Entities.Tracks;
using TrackManagement.Infrastructure.Persistence.Context;

namespace TrackManagement.Infrastructure.Persistence.Repositories;

internal sealed class TrackRepository(TrackManagemenContext db) : ITrackRepository
{
    public void Add(Track track) => db.Tracks.Add(track);

    public Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Tracks
            .Include(t => t.Distributions)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken) =>
        db.Tracks.AnyAsync(t => t.Isrc == isrc, cancellationToken);

    public async Task<PagedResponse<TrackListItemResponse>> ListAsync(
        TrackListQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<Track> tracks = db.Tracks.AsNoTracking();

        if (query.ArtistId is { } artistId)
            tracks = tracks.Where(t => t.ArtistId == artistId);

        if (query.Genre is { } genre)
            tracks = tracks.Where(t => t.Genre == genre);

        if (query.Status is { } status)
            tracks = tracks.Where(t => t.Status == status);

        var totalCount = await tracks.CountAsync(cancellationToken);

        // Deterministic order (Id as tie-breaker) so paging never repeats or skips rows.
        var rows = await tracks
            .OrderByDescending(t => t.ReleaseDate)
            .ThenBy(t => t.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.ArtistId,
                ArtistName = t.Artist.Name,
                t.Isrc,
                t.ReleaseDate,
                t.Genre,
                t.Status
            })
            .ToListAsync(cancellationToken);

        // Isrc.Value is unwrapped in memory: the column goes through a value converter.
        var items = rows
            .Select(r => new TrackListItemResponse(
                r.Id, r.Title, r.ArtistId, r.ArtistName, r.Isrc.Value, r.ReleaseDate, r.Genre, r.Status))
            .ToList();

        return new PagedResponse<TrackListItemResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<TrackDetailsResponse?> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Isrc,
                t.ReleaseDate,
                t.Genre,
                t.Status,
                Artist = new ArtistResponse(t.Artist.Id, t.Artist.Name, t.Artist.Email, t.Artist.Country),
                Distributions = t.Distributions
                    .OrderBy(d => d.SubmittedAt)
                    .ThenBy(d => d.Dsp.Name)
                    .Select(d => new TrackDistributionResponse(d.Id, d.DspId, d.Dsp.Name, d.SubmittedAt, d.Status))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new TrackDetailsResponse(
                row.Id, row.Title, row.Artist, row.Isrc.Value, row.ReleaseDate, row.Genre, row.Status, row.Distributions);
    }
}