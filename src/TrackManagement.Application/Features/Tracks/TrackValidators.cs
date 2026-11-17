using FluentValidation;

using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Features.Tracks;

public sealed class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Track.TitleMaxLength);

        RuleFor(x => x.ArtistId).NotEmpty();

        // One source of truth for the ISRC format: the Domain value object.
        RuleFor(x => x.Isrc)
            .NotEmpty()
            .Must(value => Isrc.TryCreate(value, out _))
            .WithMessage("ISRC must be 12 characters: 2-letter country, 3-character registrant, 2-digit year, 5-digit designation (e.g. EG-A01-23-00001).");

        RuleFor(x => x.ReleaseDate)
            .GreaterThanOrEqualTo(new DateOnly(1900, 1, 1))
            .WithMessage("Release date is required and must not be before 1900.");

        RuleFor(x => x.Genre).IsInEnum();
    }
}

public sealed class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public const int MaxDspsPerRequest = 50;

    public DistributeTrackRequestValidator()
    {
        RuleFor(x => x.DspIds)
            .NotNull()
            .NotEmpty().WithMessage("At least one DSP id is required.")
            .Must(ids => ids.Count <= MaxDspsPerRequest)
                .WithMessage($"At most {MaxDspsPerRequest} DSPs can be targeted per request.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("The same DSP id was specified more than once.");

        RuleForEach(x => x.DspIds).NotEmpty().WithMessage("DSP ids must not be empty.");
    }
}

public sealed class ChangeTrackStatusRequestValidator : AbstractValidator<ChangeTrackStatusRequest>
{
    public ChangeTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class TrackListQueryValidator : AbstractValidator<TrackListQuery>
{
    public const int MaxPageSize = 100;

    public TrackListQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
        RuleFor(x => x.Genre).IsInEnum();   // null passes; only validates when a value is supplied
        RuleFor(x => x.Status).IsInEnum();
    }
}