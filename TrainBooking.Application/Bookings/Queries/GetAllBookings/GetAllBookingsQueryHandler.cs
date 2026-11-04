using MediatR;
using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler
    : IRequestHandler<GetAllBookingsQuery, IReadOnlyList<BookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<IReadOnlyList<BookingResponse>> Handle(
        GetAllBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository
            .GetAllOrderedByStatusAndCreatedAtAsync(cancellationToken);

        return bookings
            .Select(b => new BookingResponse(
                b.Id,
                b.UserId,
                b.BookingCode,
                b.TotalAmount,
                b.Status.ToString(),
                b.CreatedAt,
                b.ExpiresAt))
            .ToList();
    }
}
