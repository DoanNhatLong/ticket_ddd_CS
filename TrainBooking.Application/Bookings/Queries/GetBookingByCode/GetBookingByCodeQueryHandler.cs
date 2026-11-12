using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings.Queries.GetBookingByCode;

public sealed class GetBookingByCodeQueryHandler
    : IRequestHandler<GetBookingByCodeQuery, Result<BookingDetailResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _clock;

    public GetBookingByCodeQueryHandler(IBookingRepository bookingRepository, TimeProvider clock)
    {
        _bookingRepository = bookingRepository;
        _clock = clock;
    }

    public async Task<Result<BookingDetailResponse>> Handle(
        GetBookingByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByCodeAsync(request.BookingCode, cancellationToken);

        // Another customer's booking is reported as not found, so booking codes cannot be probed.
        if (booking is null || booking.UserId != request.CustomerId)
            return BookingErrors.NotFound;

        // The background expiry runs periodically; expire an overdue booking here too so the
        // customer never sees a booking as Pending after its seat hold has run out.
        var nowUtc = _clock.GetUtcNow().UtcDateTime;
        if (booking.IsSeatHoldOverdue(nowUtc))
        {
            booking.Expire(nowUtc);
            await _bookingRepository.UpdateAsync(booking, cancellationToken);
        }

        return BookingDetailResponse.From(booking);
    }
}
