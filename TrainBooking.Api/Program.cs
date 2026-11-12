using FastEndpoints;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Polly;
using TrainBooking.Api.Background;
using TrainBooking.Api.Http;
using TrainBooking.Application;
using TrainBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddFastEndpoints();

// Releases the seats of unpaid bookings whose seat hold has run out.
builder.Services.AddHostedService<BookingExpiryService>();

// Unhandled exceptions become a 500 problem response instead of an empty body.
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddProblemDetails();

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

app.UseExceptionHandler();

app.UseFastEndpoints();

app.MapPrometheusScrapingEndpoint();

app.MapGet("/", () => "Hi");

app.Run();

// Exposes the entry point to WebApplicationFactory in the integration tests.
public partial class Program;
