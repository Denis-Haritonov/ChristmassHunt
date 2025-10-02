// Infrastructure/DependencyInjection.cs

using DataGenerator;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure.DI;

public static class EventsInfrastructureDiRegistration
{
    public static IServiceCollection AddEventsInfrastructureDI(
        this IServiceCollection services)
    {
        services.AddScoped<IEventsRepository>(_ => new RandomEventRepository("test",51.1079,17.0385));

        return services;
    }
}