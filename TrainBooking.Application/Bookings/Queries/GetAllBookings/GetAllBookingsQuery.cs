using MediatR;

namespace TrainBooking.Application.Bookings.Queries.GetAllBookings;

public sealed record GetAllBookingsQuery : IRequest<IReadOnlyList<BookingResponse>>;
