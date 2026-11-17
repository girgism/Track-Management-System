using TrackManagement.Application.Common.Exceptions;

namespace TrackManagement.Application.Tracks;

public sealed class GetTrackUseCase(ITrackRepository tracks)
{
    public async Task<TrackDetailsResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken) =>
        await tracks.GetDetailsAsync(id, cancellationToken)
        ?? throw new NotFoundException("Track", id);
}