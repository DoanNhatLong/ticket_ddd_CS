using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Bookings.Queries.GetAllBookings;
using TrainBooking.Application.Interfaces;
using TrainBooking.Domain.Bookings;
using TrainBooking.Infrastructure.Repositories.BookingRepository;
using TrainBooking.Infrastructure.Entity;
using TrainBooking.Infrastructure.Repositories.UserRepository;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký AppDbContext sử dụng kết nối MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllBookingsQuery).Assembly));

builder.Services.AddFastEndpoints();

// OpenTelemetry metrics exposed in Prometheus format at /metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("TrainBooking.Api"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddResiliencePipeline("BookingPipeline", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential
    });

    pipelineBuilder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(10),
        BreakDuration = TimeSpan.FromSeconds(30)
    });

    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(5));
});

var app = builder.Build();

app.UseFastEndpoints();

app.MapPrometheusScrapingEndpoint();

app.MapGet("/", () => "Hi");

app.Run();