using TrackManagement.Application.Common.Models;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Features.Tracks;

public interface ITrackRepository
{
    void Add(Track track);
    Task<Track?> GetWithDistributionsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsrcExistsAsync(Isrc isrc, CancellationToken cancellationToken);
    Task<PagedResponse<TrackListItemResponse>> ListAsync(TrackListQuery query, CancellationToken cancellationToken);
    Task<TrackDetailsResponse?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
}