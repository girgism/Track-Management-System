using FluentValidation;

using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;

namespace TrackManagement.Application.Tracks;

public sealed class ChangeTrackStatusUseCase(
    ITrackRepository tracks,
    IUnitOfWork unitOfWork,
    IValidator<ChangeTrackStatusRequest> validator)
{
    public async Task<TrackDetailsResponse> ExecuteAsync(
        Guid trackId,
        ChangeTrackStatusRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var track = await tracks.GetWithDistributionsAsync(trackId, cancellationToken)
                    ?? throw new NotFoundException("Track", trackId);

        // Transition rules (and the "needs a Live distribution" rule) live in the aggregate.
        track.ChangeStatus(request.Status);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await tracks.GetDetailsAsync(trackId, cancellationToken)
               ?? throw new InvalidOperationException($"Track {trackId} disappeared after saving.");
    }
}