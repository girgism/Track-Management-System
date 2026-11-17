using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TrackManagement.Application.Artists;
using TrackManagement.Application.Common.Abstractions;
using TrackManagement.Application.Dsps;
using TrackManagement.Application.Tracks;
using TrackManagement.Infrastructure.Persistence;
using TrackManagement.Infrastructure.Persistence.Context;
using TrackManagement.Infrastructure.Persistence.Repositories;
using TrackManagement.Infrastructure.Persistence.Seeding;


namespace TrackManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<TrackManagemenContext>(options => options
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(connectionString)
            .UseSeeding(DbSeeder.Seed)
            .UseAsyncSeeding(DbSeeder.SeedAsync));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IDspRepository, DspRepository>();
        return services;
    }
}