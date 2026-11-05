using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.TrainTrips;

/// <summary>
/// One scheduled run of a train from one departure station to one arrival station.
/// A ticket always covers the whole train trip (ADR 0001).
/// </summary>
public sealed class TrainTrip : Entity
{
    private readonly List<Fare> _fares;

    private TrainTrip(
        int id,
        int trainId,
        DateTime departureTime,
        DateTime arrivalTime,
        TrainTripStatus status,
        IEnumerable<Fare> fares)
        : base(id)
    {
        TrainId = trainId;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Status = status;
        _fares = fares.ToList();
    }

    public int TrainId { get; private set; }

    /// <summary>
    /// When the train trip leaves its departure station (UTC).
    /// </summary>
    public DateTime DepartureTime { get; private set; }

    /// <summary>
    /// When the train trip reaches its arrival station (UTC).
    /// </summary>
    public DateTime ArrivalTime { get; private set; }

    public TrainTripStatus Status { get; private set; }

    public IReadOnlyList<Fare> Fares => _fares;

    /// <summary>
    /// The fare this train trip asks for a seat class, or <c>null</c> when that seat class is not sold on it.
    /// </summary>
    public Fare? FareFor(int seatClassId) => _fares.SingleOrDefault(f => f.SeatClassId == seatClassId);

    /// <summary>
    /// Rebuilds a train trip from persisted state. Intended for repositories only.
    /// </summary>
    public static TrainTrip Rehydrate(
        int id,
        int trainId,
        DateTime departureTime,
        DateTime arrivalTime,
        TrainTripStatus status,
        IEnumerable<Fare> fares)
    {
        return new TrainTrip(id, trainId, departureTime, arrivalTime, status, fares);
    }
}
