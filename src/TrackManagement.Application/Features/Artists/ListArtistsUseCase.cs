namespace TrackManagement.Application.Features.Artists;

public sealed class ListArtistsUseCase(IArtistRepository artists)
{
    public Task<IReadOnlyList<ArtistResponse>> ExecuteAsync(CancellationToken cancellationToken) =>
        artists.ListAsync(cancellationToken);
}