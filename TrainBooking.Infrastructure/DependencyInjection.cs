using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainBooking.Domain.Bookings;
using TrainBooking.Domain.Users;
using TrainBooking.Infrastructure.Entity;
using TrainBooking.Infrastructure.Repositories.BookingRepository;
using TrainBooking.Infrastructure.Repositories.UserRepository;

namespace TrainBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services: MySQL DbContext, repositories and Redis cache.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddCaching(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "For local development set it with 'dotnet user-secrets'.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        // Inject IDistributedCache wherever caching is needed.
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "TrainBooking:";
        });
    }
}
