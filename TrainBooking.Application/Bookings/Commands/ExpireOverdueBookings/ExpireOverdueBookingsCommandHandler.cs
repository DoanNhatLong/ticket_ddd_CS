using MediatR;
using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings.Commands.ExpireOverdueBookings;

public sealed class ExpireOverdueBookingsCommandHandler : IRequestHandler<ExpireOverdueBookingsCommand, int>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _clock;

    public ExpireOverdueBookingsCommandHandler(IBookingRepository bookingRepository, TimeProvider clock)
    {
        _bookingRepository = bookingRepository;
        _clock = clock;
    }

    public async Task<int> Handle(ExpireOverdueBookingsCommand command, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.GetUtcNow().UtcDateTime;
        var overdue = await _bookingRepository.GetOverdueAsync(nowUtc, command.MaxCount, cancellationToken);

        var expired = 0;
        foreach (var booking in overdue)
        {
            if (booking.Expire(nowUtc).IsFailure)
                continue;

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            expired++;
        }

        return expired;
    }
}
