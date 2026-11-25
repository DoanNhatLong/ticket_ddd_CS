namespace TrainBooking.Application.TrainTrips.Search;

/// <summary>
/// What to search for; a null value means "any".
/// </summary>
/// <param name="Text">Free text: station, city or train; accents and small typos are tolerated.</param>
/// <param name="DepartureStationCode">Exact station code, upper case.</param>
/// <param name="ArrivalStationCode">Exact station code, upper case.</param>
/// <param name="DepartureDate">Departure day (UTC).</param>
/// <param name="DepartsAfterUtc">Only train trips leaving after this moment.</param>
/// <param name="Page">Page number, from 1.</param>
/// <param name="PageSize">Number of train trips per page.</param>
public sealed record TrainTripSearchCriteria(
    string? Text,
    string? DepartureStationCode,
    string? ArrivalStationCode,
    DateOnly? DepartureDate,
    DateTime DepartsAfterUtc,
    int Page,
    int PageSize);
