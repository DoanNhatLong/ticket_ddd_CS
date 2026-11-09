using TrainBooking.Domain.Bookings;

namespace TrainBooking.Application.Bookings;

/// <summary>
/// A booking with its tickets, as returned to the customer who owns it.
/// </summary>
public sealed record BookingDetailResponse(
    string BookingCode,
    string Status,
    decimal TotalAmount,
    DateTime? CreatedAt,
    DateTime ExpiresAt,
    IReadOnlyList<TicketResponse> Tickets)
{
    public static BookingDetailResponse From(Booking booking) => new(
        booking.BookingCode,
        booking.Status.ToString(),
        booking.TotalAmount,
        booking.CreatedAt,
        booking.ExpiresAt,
        booking.Tickets.Select(TicketResponse.From).ToList());
}

public sealed record TicketResponse(
    int TicketId,
    int TrainTripId,
    int SeatId,
    string PassengerName,
    string PassengerIdNumber,
    decimal Price,
    string Status)
{
    public static TicketResponse From(Ticket ticket) => new(
        ticket.Id,
        ticket.TrainTripId,
        ticket.SeatId,
        ticket.PassengerName,
        ticket.PassengerIdNumber,
        ticket.Price,
        ticket.Status.ToString());
}
