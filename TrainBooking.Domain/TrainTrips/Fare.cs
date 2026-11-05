namespace TrainBooking.Domain.TrainTrips;

/// <summary>
/// The price a train trip asks for one seat class.
/// </summary>
public sealed record Fare(int SeatClassId, decimal Price);
