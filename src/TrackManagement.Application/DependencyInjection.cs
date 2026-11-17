using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using TrackManagement.Application.Artists;
using TrackManagement.Application.Tracks;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();

        // Registered explicitly: the list of use cases is the list of things the system can do.
        services.AddScoped<CreateArtistUseCase>();
        services.AddScoped<ListArtistsUseCase>();
        services.AddScoped<CreateTrackUseCase>();
        services.AddScoped<ListTracksUseCase>();
        services.AddScoped<GetTrackUseCase>();
        services.AddScoped<DistributeTrackUseCase>();
        services.AddScoped<ChangeTrackStatusUseCase>();

        return services;
    }
}