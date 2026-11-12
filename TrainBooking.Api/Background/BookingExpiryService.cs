using MediatR;
using TrainBooking.Application.Bookings.Commands.ExpireOverdueBookings;

namespace TrainBooking.Api.Background;

/// <summary>
/// Periodically expires unpaid bookings whose seat hold has run out, so their seats go back on sale
/// even when nobody books them again. Interval: <c>BookingExpiry:IntervalSeconds</c> (0 disables it).
/// </summary>
public sealed class BookingExpiryService : BackgroundService
{
    private const int DefaultIntervalSeconds = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpiryService> _logger;
    private readonly TimeSpan _interval;

    public BookingExpiryService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BookingExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(
            configuration.GetValue("BookingExpiry:IntervalSeconds", DefaultIntervalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_interval <= TimeSpan.Zero)
        {
            _logger.LogInformation("Booking expiry is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(_interval);
        do
        {
            await ExpireOverdueBookingsAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExpireOverdueBookingsAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var expired = await sender.Send(new ExpireOverdueBookingsCommand(), stoppingToken);
            if (expired > 0)
                _logger.LogInformation("Expired {Count} overdue bookings.", expired);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception exception)
        {
            // Keep the service alive; the next tick retries.
            _logger.LogError(exception, "Expiring overdue bookings failed.");
        }
    }
}
