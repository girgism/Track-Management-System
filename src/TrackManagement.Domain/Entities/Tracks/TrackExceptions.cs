using global::TrackManagement.Domain.Common;
using global::TrackManagement.Domain.Entities.Distributions;

namespace TrackManagement.Domain.Entities.Tracks;

public sealed class InvalidTrackStatusTransitionException : DomainException
{
    public InvalidTrackStatusTransitionException(TrackStatus from, TrackStatus to, string message)
        : base("track.invalid_status_transition", message)
    {
        From = from;
        To = to;
    }

    public TrackStatus From { get; }
    public TrackStatus To { get; }
}

public sealed class TrackNotDistributableException : DomainException
{
    public TrackNotDistributableException(Guid trackId, TrackStatus status)
        : base("track.not_distributable",
               $"Track {trackId} is {status} and cannot be distributed. Only Submitted or Distributed tracks can be sent to DSPs.")
    {
    }
}

public sealed class DuplicateDistributionException : DomainException
{
    public DuplicateDistributionException(Guid trackId, IReadOnlyCollection<Guid> dspIds)
        : base("track.duplicate_distribution",
               $"Track {trackId} is already associated with DSP(s): {string.Join(", ", dspIds)}. Only rejected distributions can be resubmitted.")
    {
        DspIds = dspIds;
    }

    public IReadOnlyCollection<Guid> DspIds { get; }
}

public sealed class DistributionNotFoundException : DomainException
{
    public DistributionNotFoundException(Guid trackId, Guid dspId)
        : base("track.distribution_not_found",
               $"Track {trackId} has no distribution for DSP {dspId}.")
    {
    }
}

public sealed class InvalidDistributionStatusTransitionException : DomainException
{
    public InvalidDistributionStatusTransitionException(DistributionStatus from, DistributionStatus to)
        : base("distribution.invalid_status_transition",
               $"A distribution cannot change from {from} to {to}.")
    {
    }
}