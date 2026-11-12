using MediatR;
using TrainBooking.Application.Abstractions;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Bookings.Commands.CancelBooking;

/// <summary>
/// Cancels one of the customer's own unpaid bookings before its seat hold runs out, releasing its seats.
/// </summary>
public sealed record CancelBookingCommand(int? CustomerId, string BookingCode)
    : IRequest<Result<BookingDetailResponse>>, ICustomerRequest;
