using MediatR;
using TrainBooking.Application.Abstractions;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Bookings.Queries.GetBookingByCode;

/// <summary>
/// Returns one of the customer's own bookings by its booking code.
/// </summary>
public sealed record GetBookingByCodeQuery(int? CustomerId, string BookingCode)
    : IRequest<Result<BookingDetailResponse>>, ICustomerRequest;
