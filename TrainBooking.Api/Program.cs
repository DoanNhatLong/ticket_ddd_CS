using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Interfaces;
using TrainBooking.Infrastructure.Entity;
using TrainBooking.Infrastructure.Repositories.UserRepository;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký AppDbContext sử dụng kết nối MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUserRepository, UserRepository>();

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