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

        // Explicit server version instead of ServerVersion.AutoDetect: AutoDetect opens a DB
        // connection every time DbContext options are built (once per scope/request).
        var serverVersionText = configuration["Database:ServerVersion"];
        if (string.IsNullOrWhiteSpace(serverVersionText))
        {
            throw new InvalidOperationException(
                "'Database:ServerVersion' is not configured (e.g. \"8.0.46-mysql\").");
        }

        var serverVersion = ServerVersion.Parse(serverVersionText);

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

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
