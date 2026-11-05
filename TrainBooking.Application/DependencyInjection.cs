using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrainBooking.Application.Abstractions.Behaviors;

namespace TrainBooking.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer services (MediatR handlers, pipeline behaviors, the clock).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(CustomerAuthenticationBehavior<,>));
        });

        // All code reads the current time through TimeProvider so tests can replace the clock.
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
