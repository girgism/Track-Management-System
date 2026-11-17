using FluentValidation;

using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Models;
using TrackManagement.Application.Features.DSPs;

namespace TrackManagement.Application.Features.Tracks;

public sealed class DistributeTrackUseCase(
    ITrackRepository tracks,
    IDspRepository dsps,
    IUnitOfWork unitOfWork,
    IValidator<DistributeTrackRequest> validator,
    TimeProvider timeProvider)
{
    public async Task<CommandResponse> ExecuteAsync(
        Guid trackId,
        DistributeTrackRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        // Tracked, with distributions loaded: the aggregate needs them to detect duplicates.
        var track = await tracks.GetWithDistributionsAsync(trackId, cancellationToken)
                    ?? throw new NotFoundException("Track", trackId);

        var existing = await dsps.GetExistingIdsAsync(request.DspIds, cancellationToken);
        var unknown = request.DspIds.Except(existing).ToList();

        if (unknown.Count > 0)
            throw new InvalidReferenceException("dspIds", $"Unknown DSP id(s): {string.Join(", ", unknown)}.");

        // Draft / already-associated rules live in the aggregate and throw domain exceptions.
        track.SubmitToDsps(request.DspIds, timeProvider.GetUtcNow());

        // One SaveChanges = one transaction: all DSP rows are saved, or none.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommandResponse(track.Id, $"Track submitted to {request.DspIds.Count} DSP(s) successfully.");
    }
}