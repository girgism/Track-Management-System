using FluentValidation;

using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Models;
using TrackManagement.Application.Features.Artists;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Features.Tracks;

public sealed class CreateTrackUseCase(
    ITrackRepository tracks,
    IArtistRepository artists,
    IUnitOfWork unitOfWork,
    IValidator<CreateTrackRequest> validator)
{
    public async Task<CommandResponse> ExecuteAsync(CreateTrackRequest request, CancellationToken cancellationToken)
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

        return new CommandResponse(track.Id, "Track created successfully.");
    }
}