using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Trains;

/// <summary>
/// A seat on a train, identified to passengers by its carriage number and seat number.
/// </summary>
public sealed class Seat : Entity
{
    private Seat(int id, int trainId, int carriageNumber, string seatNumber, SeatClass seatClass)
        : base(id)
    {
        TrainId = trainId;
        CarriageNumber = carriageNumber;
        SeatNumber = seatNumber;
        SeatClass = seatClass;
    }

    /// <summary>
    /// The train this seat belongs to.
    /// </summary>
    public int TrainId { get; private set; }

    public int CarriageNumber { get; private set; }

    public string SeatNumber { get; private set; }

    public SeatClass SeatClass { get; private set; }

    /// <summary>
    /// Rebuilds a seat from persisted state. Intended for repositories only.
    /// </summary>
    public static Seat Rehydrate(int id, int trainId, int carriageNumber, string seatNumber, SeatClass seatClass)
    {
        return new Seat(id, trainId, carriageNumber, seatNumber, seatClass);
    }
}
