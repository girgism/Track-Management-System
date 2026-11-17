
using Microsoft.EntityFrameworkCore;

using TrackManagement.Domain.Entities.Artists;
using TrackManagement.Domain.Entities.DigitalServiceProviders;
using TrackManagement.Domain.Entities.Distributions;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Domain.Interfaces;

public interface ITrackManagementContext
{
    DbSet<Artist> Artists { get; }
    DbSet<Track> Tracks { get; }
    DbSet<DSP> DSPs { get; }
    DbSet<TrackDistribution> TrackDistributions { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
