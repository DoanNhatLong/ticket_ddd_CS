using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Bookings;

/// <summary>
/// The right for one passenger to occupy one seat on one train trip. Child entity of <see cref="Booking"/>.
/// </summary>
public sealed class Ticket : Entity
{
    private Ticket(
        int id,
        int trainTripId,
        int seatId,
        string passengerName,
        string passengerIdNumber,
        decimal price,
        TicketStatus status)
        : base(id)
    {
        TrainTripId = trainTripId;
        SeatId = seatId;
        PassengerName = passengerName;
        PassengerIdNumber = passengerIdNumber;
        Price = price;
        Status = status;
    }

    public int TrainTripId { get; private set; }

    public int SeatId { get; private set; }

    public string PassengerName { get; private set; }

    /// <summary>
    /// Number of the passenger's identity document.
    /// </summary>
    public string PassengerIdNumber { get; private set; }

    /// <summary>
    /// Ticket price: the fare that applied when the ticket was booked; never changes afterwards.
    /// </summary>
    public decimal Price { get; private set; }

    public TicketStatus Status { get; private set; }

    internal static Ticket CreateHeld(
        int trainTripId,
        int seatId,
        string passengerName,
        string passengerIdNumber,
        decimal price)
    {
        return new Ticket(0, trainTripId, seatId, passengerName, passengerIdNumber, price, TicketStatus.Held);
    }

    /// <summary>
    /// Rebuilds a ticket from persisted state. Intended for repositories only.
    /// </summary>
    public static Ticket Rehydrate(
        int id,
        int trainTripId,
        int seatId,
        string passengerName,
        string passengerIdNumber,
        decimal price,
        TicketStatus status)
    {
        return new Ticket(id, trainTripId, seatId, passengerName, passengerIdNumber, price, status);
    }
}
