using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler
    : IRequestHandler<CancelBookingCommand, Result<BookingDetailResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _clock;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository, TimeProvider clock)
    {
        _bookingRepository = bookingRepository;
        _clock = clock;
    }

    public async Task<Result<BookingDetailResponse>> Handle(
        CancelBookingCommand command,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByCodeAsync(command.BookingCode, cancellationToken);

        // Another customer's booking is reported as not found, so booking codes cannot be probed.
        if (booking is null || booking.UserId != command.CustomerId)
            return BookingErrors.NotFound;

        var nowUtc = _clock.GetUtcNow().UtcDateTime;

        if (booking.IsSeatHoldOverdue(nowUtc))
        {
            // Too late to cancel: record the expiry the background service would have recorded.
            booking.Expire(nowUtc);
            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            return BookingErrors.Expired;
        }

        var cancelled = booking.CancelUnpaid(nowUtc);
        if (cancelled.IsFailure)
            return cancelled.Error;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        return BookingDetailResponse.From(booking);
    }
}
