using Microsoft.Extensions.DependencyInjection;

namespace TrainBooking.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer services (MediatR handlers, etc.).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
