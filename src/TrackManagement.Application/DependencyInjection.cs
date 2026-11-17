using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using TrackManagement.Application.Features.Artists;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();

        // Registered automatically: the list of use cases is the list of things the system can do.
        services.Scan(scan => scan
                    .FromAssemblyOf<CreateArtistUseCase>()
                    .AddClasses(classes => classes.Where(type =>
                        type.Name.EndsWith("UseCase")))
                    .AsSelf()
                    .WithScopedLifetime());

        return services;
    }
}