using FluentValidation;

using TrackManagement.Application.Common.Models;

namespace TrackManagement.Application.Tracks;

public sealed class ListTracksUseCase(ITrackRepository tracks, IValidator<TrackListQuery> validator)
{
    public async Task<PagedResponse<TrackListItemResponse>> ExecuteAsync(
        TrackListQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        return await tracks.ListAsync(query, cancellationToken);
    }
}