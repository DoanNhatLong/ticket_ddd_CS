namespace TrainBooking.Application.Bookings.Queries.GetAllBookings;

public sealed record BookingResponse(
    int Id,
    int UserId,
    string BookingCode,
    decimal TotalAmount,
    string Status,
    DateTime? CreatedAt,
    DateTime ExpiresAt);
