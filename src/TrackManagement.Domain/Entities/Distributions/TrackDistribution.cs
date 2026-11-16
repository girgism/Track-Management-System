using TrackManagement.Domain.Entities.DigitalServiceProviders;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Domain.Entities.Distributions;

public sealed class TrackDistribution
{
    private TrackDistribution() { }

    public Guid Id { get; private set; }
    public Guid TrackId { get; private set; }
    public Guid DspId { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DistributionStatus Status { get; private set; }

    public Track Track { get; private set; } = null!;
    public Dsp Dsp { get; private set; } = null!;

    public static TrackDistribution Create(Guid trackId, Guid dspId, DateTimeOffset submittedAt) => new()
    {
        Id = Guid.CreateVersion7(),
        TrackId = trackId,
        DspId = dspId,
        SubmittedAt = submittedAt,
        Status = DistributionStatus.Pending
    };

    public void Resubmit(DateTimeOffset submittedAt)
    {
        if (Status != DistributionStatus.Rejected)
            throw new InvalidDistributionStatusTransitionException(Status, DistributionStatus.Pending);

        Status = DistributionStatus.Pending;
        SubmittedAt = submittedAt;
    }

    public void MarkLive()
    {
        if (Status != DistributionStatus.Pending)
            throw new InvalidDistributionStatusTransitionException(Status, DistributionStatus.Live);

        Status = DistributionStatus.Live;
    }

    public void Reject()
    {
        if (Status != DistributionStatus.Pending)
            throw new InvalidDistributionStatusTransitionException(Status, DistributionStatus.Rejected);

        Status = DistributionStatus.Rejected;
    }
}