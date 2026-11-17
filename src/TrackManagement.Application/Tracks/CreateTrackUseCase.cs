using FluentValidation;

using TrackManagement.Application.Artists;
using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Tracks;

public sealed class CreateTrackUseCase(
    ITrackRepository tracks,
    IArtistRepository artists,
    IUnitOfWork unitOfWork,
    IValidator<CreateTrackRequest> validator)
{
    public async Task<TrackDetailsResponse> ExecuteAsync(CreateTrackRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await artists.ExistsAsync(request.ArtistId, cancellationToken))
            throw new InvalidReferenceException("artistId", $"Artist '{request.ArtistId}' does not exist.");

        var isrc = Isrc.Create(request.Isrc);

        if (await tracks.IsrcExistsAsync(isrc, cancellationToken))
            throw new ConflictException("track.duplicate_isrc", $"A track with ISRC '{isrc}' already exists.");

        var track = Track.Create(request.Title, request.ArtistId, isrc, request.ReleaseDate, request.Genre);

        tracks.Add(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await tracks.GetDetailsAsync(track.Id, cancellationToken)
               ?? throw new InvalidOperationException($"Track {track.Id} was saved but could not be read back.");
    }
}