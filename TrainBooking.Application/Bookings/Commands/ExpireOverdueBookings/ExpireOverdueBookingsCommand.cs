using MediatR;

namespace TrainBooking.Application.Bookings.Commands.ExpireOverdueBookings;

/// <summary>
/// Expires up to <paramref name="MaxCount"/> unpaid bookings whose seat hold has run out and puts
/// their seats back on sale. Returns how many bookings were expired.
/// </summary>
public sealed record ExpireOverdueBookingsCommand(int MaxCount = 100) : IRequest<int>;
