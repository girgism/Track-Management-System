using global::TrackManagement.Application.Common.Auth;
using global::TrackManagement.Infrastructure.Persistence.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TrackManagement.Application.Common.Abstractions;
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

        // Read and validated eagerly: a bad or missing secret stops the app at startup.
        var jwtOptions = JwtOptions.Load(configuration);
        var adminSeed = AdminSeedOptions.Load(configuration);

        // The hasher is stateless, so a single instance serves both DI and the seeding callbacks.
        var passwordHasher = new Pbkdf2PasswordHasher();

        services.AddSingleton(jwtOptions);
        services.AddSingleton<IPasswordHasher>(passwordHasher);
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddDbContext<TrackManagementContext>(options => options
            .UseSqlServer(connectionString)
            .UseSeeding((context, storeOperationPerformed) =>
            {
                DbSeeder.Seed(context, storeOperationPerformed);
                AdminSeeder.Seed(context, adminSeed, passwordHasher);
            })
            .UseAsyncSeeding(async (context, storeOperationPerformed, cancellationToken) =>
            {
                await DbSeeder.SeedAsync(context, storeOperationPerformed, cancellationToken);
                await AdminSeeder.SeedAsync(context, adminSeed, passwordHasher, cancellationToken);
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registered automatically: the list of use cases is the list of things the system can do.
        services.Scan(scan => scan
                  .FromAssemblyOf<ArtistRepository>()
                  .AddClasses(
                      classes => classes.Where(type =>
                          type.Name.EndsWith("Repository")),
                      publicOnly: false)
                  .AsImplementedInterfaces()
                  .WithScopedLifetime());


        return services;
    }
}