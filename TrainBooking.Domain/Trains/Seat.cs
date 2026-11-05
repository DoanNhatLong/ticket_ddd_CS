using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Trains;

/// <summary>
/// A seat on a train, identified to passengers by its carriage number and seat number.
/// </summary>
public sealed class Seat : Entity
{
    private Seat(int id, int carriageNumber, string seatNumber, SeatClass seatClass)
        : base(id)
    {
        CarriageNumber = carriageNumber;
        SeatNumber = seatNumber;
        SeatClass = seatClass;
    }

    public int CarriageNumber { get; private set; }

    public string SeatNumber { get; private set; }

    public SeatClass SeatClass { get; private set; }

    /// <summary>
    /// Rebuilds a seat from persisted state. Intended for repositories only.
    /// </summary>
    public static Seat Rehydrate(int id, int carriageNumber, string seatNumber, SeatClass seatClass)
    {
        return new Seat(id, carriageNumber, seatNumber, seatClass);
    }
}
