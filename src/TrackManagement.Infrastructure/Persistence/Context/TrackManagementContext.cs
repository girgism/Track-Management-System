using Microsoft.EntityFrameworkCore;

using TrackManagement.Domain.Entities.Artists;
using TrackManagement.Domain.Entities.DigitalServiceProviders;
using TrackManagement.Domain.Entities.Distributions;
using TrackManagement.Domain.Entities.Tracks;
using TrackManagement.Domain.Entities.Users;

namespace TrackManagement.Infrastructure.Persistence.Context;

public sealed class TrackManagementContext(DbContextOptions<TrackManagementContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackManagementContext).Assembly);
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<DSP> DSPs => Set<DSP>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackDistribution> TrackDistributions => Set<TrackDistribution>();

    public DbSet<User> Users => Set<User>();

}