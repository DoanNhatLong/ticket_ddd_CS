using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainBooking.Application.Stations;
using TrainBooking.Application.TrainTrips.Search;
using TrainBooking.Domain.Bookings;
using TrainBooking.Domain.TrainTrips;
using TrainBooking.Domain.Trains;
using TrainBooking.Domain.Users;
using TrainBooking.Infrastructure.Entity;
using TrainBooking.Infrastructure.Repositories.BookingRepository;
using TrainBooking.Infrastructure.Repositories.SeatRepository;
using TrainBooking.Infrastructure.Repositories.TrainTripRepository;
using TrainBooking.Infrastructure.Repositories.UserRepository;
using TrainBooking.Infrastructure.Search;

namespace TrainBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services: MySQL DbContext, repositories, Redis cache and Elasticsearch client.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddCaching(services, configuration);
        AddSearch(services, configuration);

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
        services.AddScoped<ITrainTripRepository, TrainTripRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
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

    private static void AddSearch(IServiceCollection services, IConfiguration configuration)
    {
        var urlText = configuration["Elasticsearch:Url"];
        if (!Uri.TryCreate(urlText, UriKind.Absolute, out var url))
        {
            throw new InvalidOperationException(
                "'Elasticsearch:Url' is not configured or is not a valid URL (e.g. \"http://localhost:9200\").");
        }

        var trainTripIndex = configuration["Elasticsearch:TrainTripIndex"];
        if (string.IsNullOrWhiteSpace(trainTripIndex))
        {
            throw new InvalidOperationException(
                "'Elasticsearch:TrainTripIndex' is not configured (e.g. \"train-trips\").");
        }

        var stationIndex = configuration["Elasticsearch:StationIndex"];
        if (string.IsNullOrWhiteSpace(stationIndex))
        {
            throw new InvalidOperationException(
                "'Elasticsearch:StationIndex' is not configured (e.g. \"stations\").");
        }

        services.AddSingleton(new ElasticsearchOptions(url, trainTripIndex, stationIndex));

        // The client is thread-safe and manages its own connections: one instance for the whole app.
        // Creating it does not connect, so the API still starts when Elasticsearch is down.
        services.AddSingleton(new ElasticsearchClient(
            new ElasticsearchClientSettings(url).RequestTimeout(TimeSpan.FromSeconds(5))));

        services.AddSingleton<SearchIndexBuilder>();
        services.AddScoped<IStationSuggester, ElasticsearchStationSuggester>();
        services.AddScoped<ITrainTripSearch, ElasticsearchTrainTripSearch>();
    }
}
