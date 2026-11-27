using MediatR;
using TrainBooking.Application.Abstractions;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Bookings.Commands.CreateBooking;

/// <summary>
/// Books one or more seats for the customer and holds them until the booking is paid or the hold runs out.
/// </summary>
public sealed record CreateBookingCommand(int? CustomerId, IReadOnlyList<CreateBookingTicket> Tickets)
    : IRequest<Result<BookingDetailResponse>>, ICustomerRequest;

/// <param name="ExpectedPrice">
/// The ticket price the customer was shown. Required: the booking is refused with
/// <c>Booking.PriceChanged</c> when the current fare differs.
/// </param>
public sealed record CreateBookingTicket(
    int TrainTripId,
    int SeatId,
    string? PassengerName,
    string? PassengerIdNumber,
    decimal? ExpectedPrice);
