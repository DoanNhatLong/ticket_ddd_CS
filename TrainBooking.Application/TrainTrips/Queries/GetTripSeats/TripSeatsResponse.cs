namespace TrainBooking.Application.TrainTrips.Queries.GetTripSeats;

public sealed record TripSeatsResponse(
    int TrainTripId,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    IReadOnlyList<TripSeatResponse> Seats);

/// <param name="Fare">The train trip's fare for this seat's seat class, or <c>null</c> when it is not sold.</param>
/// <param name="IsFree">Whether nobody holds or owns this seat on the train trip right now.</param>
public sealed record TripSeatResponse(
    int SeatId,
    int CarriageNumber,
    string SeatNumber,
    string SeatClassCode,
    string SeatClassName,
    decimal? Fare,
    bool IsFree);
