namespace TrainBooking.Application.TrainTrips.Search;

/// <summary>
/// One page of search results.
/// </summary>
public sealed record TrainTripSearchPage(
    IReadOnlyList<TrainTripSearchItem> Items,
    long Total,
    int Page,
    int PageSize);

public sealed record TrainTripSearchItem(
    int TrainTripId,
    string TrainCode,
    string TrainName,
    TrainTripSearchStation Departure,
    TrainTripSearchStation Arrival,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Status,
    decimal? LowestFare);

public sealed record TrainTripSearchStation(string Code, string Name, string City);
