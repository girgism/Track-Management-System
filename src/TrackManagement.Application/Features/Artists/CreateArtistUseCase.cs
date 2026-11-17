using FluentValidation;

using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Models;
using TrackManagement.Domain.Entities.Artists;

namespace TrackManagement.Application.Features.Artists;

public sealed class CreateArtistUseCase(IArtistRepository artists,
                                        IUnitOfWork unitOfWork,
                                        IValidator<CreateArtistRequest> validator)
{
    public async Task<CommandResponse> ExecuteAsync(CreateArtistRequest request,
                                                    CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await artists.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("artist.duplicate_email", $"An artist with email '{email}' already exists.");

        var artist = Artist.Create(request.Name, email, request.Country);

        artists.Add(artist);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommandResponse(artist.Id, "Artist created successfully.");
    }
}