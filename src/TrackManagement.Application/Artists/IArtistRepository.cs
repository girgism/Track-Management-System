using TrackManagement.Domain.Entities.Artists;

namespace TrackManagement.Application.Artists;

public interface IArtistRepository
{
    void Add(Artist artist);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<IReadOnlyList<ArtistResponse>> ListAsync(CancellationToken cancellationToken);
}