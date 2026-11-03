using FastEndpoints;
using TrainBooking.Application.Interfaces;
using TrainBooking.Infrastructure.Repositories.UserRepository;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository, TrainBooking.Infrastructure.Repositories.UserRepository.UserRepository>();

builder.Services.AddFastEndpoints();

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

app.MapGet("/", () => "Hi");

app.Run();
