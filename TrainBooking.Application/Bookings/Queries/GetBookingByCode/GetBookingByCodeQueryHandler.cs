using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings.Queries.GetBookingByCode;

public sealed class GetBookingByCodeQueryHandler
    : IRequestHandler<GetBookingByCodeQuery, Result<BookingDetailResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByCodeQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingDetailResponse>> Handle(
        GetBookingByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByCodeAsync(request.BookingCode, cancellationToken);

        // Another customer's booking is reported as not found, so booking codes cannot be probed.
        if (booking is null || booking.UserId != request.CustomerId)
            return BookingErrors.NotFound;

        return BookingDetailResponse.From(booking);
    }
}
