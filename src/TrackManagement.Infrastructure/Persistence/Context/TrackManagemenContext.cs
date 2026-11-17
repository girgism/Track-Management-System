using Microsoft.EntityFrameworkCore;

using TrackManagement.Domain.Entities.Artists;
using TrackManagement.Domain.Entities.DigitalServiceProviders;
using TrackManagement.Domain.Entities.Distributions;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Infrastructure.Persistence.Context;

public sealed class TrackManagemenContext(DbContextOptions<TrackManagemenContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackManagemenContext).Assembly);
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<DSP> DSPs => Set<DSP>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackDistribution> TrackDistributions => Set<TrackDistribution>();

}