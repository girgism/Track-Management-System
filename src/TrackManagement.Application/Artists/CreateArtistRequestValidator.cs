using FluentValidation;

using TrackManagement.Domain.Entities.Artists;

namespace TrackManagement.Application.Artists;

public sealed class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Artist.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(Artist.EmailMaxLength)
            .EmailAddress();

        RuleFor(x => x.Country)
            .NotEmpty()
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("Country must be a 2-letter ISO code (e.g. EG).");
    }
}