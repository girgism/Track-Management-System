using TrackManagement.Application.Common.Models;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Tracks;

public interface ITrackRepository
{
    // Write side: the aggregate, tracked, with its distributions loaded.
    void Add(Track track);
    Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken);

    // Read side: projections straight into DTOs, no tracking.
    Task<PagedResponse<TrackListItemResponse>> ListAsync(TrackListQuery query, CancellationToken cancellationToken);
    Task<TrackDetailsResponse?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
}