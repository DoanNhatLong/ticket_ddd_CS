using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Bookings;

/// <summary>
/// Booking aggregate root.
/// </summary>
public sealed class Booking : Entity
{
    private Booking(
        int id,
        int userId,
        string bookingCode,
        decimal totalAmount,
        BookingStatus status,
        DateTime? createdAt,
        DateTime expiresAt)
        : base(id)
    {
        UserId = userId;
        BookingCode = bookingCode;
        TotalAmount = totalAmount;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public int UserId { get; private set; }

    public string BookingCode { get; private set; }

    public decimal TotalAmount { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Rebuilds an existing booking from persisted state.
    /// Intended for repositories only; new bookings must go through a dedicated factory method.
    /// </summary>
    public static Booking Rehydrate(
        int id,
        int userId,
        string bookingCode,
        decimal totalAmount,
        BookingStatus status,
        DateTime? createdAt,
        DateTime expiresAt)
    {
        return new Booking(id, userId, bookingCode, totalAmount, status, createdAt, expiresAt);
    }
}
