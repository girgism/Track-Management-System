using global::TrackManagement.Domain.Common;
using global::TrackManagement.Domain.Entities.Artists;
using global::TrackManagement.Domain.Entities.Distributions;

namespace TrackManagement.Domain.Entities.Tracks;

public sealed class Track
{
    public const int TitleMaxLength = 300;

    private Track() { }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public Guid ArtistId { get; private set; }
    public Isrc Isrc { get; private set; } = null!;
    public DateOnly ReleaseDate { get; private set; }
    public Genre Genre { get; private set; }
    public TrackStatus Status { get; private set; }

    public Artist Artist { get; private set; } = null!;
    private readonly List<TrackDistribution> _distributions = [];
    public IReadOnlyCollection<TrackDistribution> Distributions => _distributions.AsReadOnly();

    public static Track Create(string? title, Guid artistId, Isrc isrc, DateOnly releaseDate, Genre genre)
    {
        ArgumentNullException.ThrowIfNull(isrc);

        if (releaseDate == default)
            throw new DomainValidationException(nameof(releaseDate), "Release date is required.");

        if (!Enum.IsDefined(genre))
            throw new DomainValidationException(nameof(genre), "Genre is not a valid value.");

        return new Track
        {
            Id = Guid.CreateVersion7(),
            Title = Guard.NotEmpty(title, nameof(title), TitleMaxLength),
            ArtistId = Guard.NotEmpty(artistId, nameof(artistId)),
            Isrc = isrc,
            ReleaseDate = releaseDate,
            Genre = genre,
            Status = TrackStatus.Draft
        };
    }

    public void ChangeStatus(TrackStatus target)
    {
        if (!Enum.IsDefined(target))
            throw new DomainValidationException(nameof(target), "Status is not a valid value.");

        if (target == Status)
            return;

        var error = (Status, target) switch
        {
            (TrackStatus.Draft, TrackStatus.Submitted) => null,

            (TrackStatus.Submitted, TrackStatus.Draft) =>
                _distributions.Count == 0
                    ? null
                    : "A track that already has distribution records cannot return to Draft.",

            (TrackStatus.Submitted, TrackStatus.Distributed) =>
                _distributions.Any(d => d.Status == DistributionStatus.Live)
                    ? null
                    : "A track can only be marked Distributed once at least one DSP distribution is Live.",

            _ => $"Changing a track from {Status} to {target} is not allowed."
        };

        if (error is not null)
            throw new InvalidTrackStatusTransitionException(Status, target, error);

        Status = target;
    }

    /// <summary>
    /// Submits the track to one or more DSPs, all-or-nothing. Rejected associations are resubmitted;
    /// Pending/Live ones are duplicates. Returns the created or resubmitted distributions.
    /// </summary>
    public IReadOnlyList<TrackDistribution> SubmitToDsps(IReadOnlyCollection<Guid> dspIds, DateTimeOffset submittedAt)
    {
        ArgumentNullException.ThrowIfNull(dspIds);

        if (Status == TrackStatus.Draft)
            throw new TrackNotDistributableException(Id, Status);

        if (dspIds.Count == 0)
            throw new DomainValidationException(nameof(dspIds), "At least one DSP is required.");

        if (dspIds.Any(id => id == Guid.Empty))
            throw new DomainValidationException(nameof(dspIds), "DSP ids must not be empty.");

        if (dspIds.Distinct().Count() != dspIds.Count)
            throw new DomainValidationException(nameof(dspIds), "The same DSP was specified more than once.");

        var existing = _distributions.ToDictionary(d => d.DspId);

        // Validate everything first so a failure leaves the aggregate untouched.
        var duplicates = dspIds
            .Where(id => existing.TryGetValue(id, out var d) && d.Status != DistributionStatus.Rejected)
            .ToList();

        if (duplicates.Count > 0)
            throw new DuplicateDistributionException(Id, duplicates);

        var affected = new List<TrackDistribution>(dspIds.Count);

        foreach (var dspId in dspIds)
        {
            if (existing.TryGetValue(dspId, out var rejected))
            {
                rejected.Resubmit(submittedAt);
                affected.Add(rejected);
            }
            else
            {
                var distribution = TrackDistribution.Create(Id, dspId, submittedAt);
                _distributions.Add(distribution);
                affected.Add(distribution);
            }
        }

        return affected;
    }

    /// <summary>A DSP confirmed the track is live. Does not change the track status (see ChangeStatus).</summary>
    public void MarkDistributionLive(Guid dspId) => GetDistribution(dspId).MarkLive();

    /// <summary>A DSP rejected the track. The distribution may later be resubmitted.</summary>
    public void RejectDistribution(Guid dspId) => GetDistribution(dspId).Reject();

    private TrackDistribution GetDistribution(Guid dspId) =>
        _distributions.FirstOrDefault(d => d.DspId == dspId)
        ?? throw new DistributionNotFoundException(Id, dspId);
}
