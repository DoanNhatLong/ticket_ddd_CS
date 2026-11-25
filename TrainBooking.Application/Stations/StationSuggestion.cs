namespace TrainBooking.Application.Stations;

/// <summary>
/// A station offered to the customer. Name and city are for display; the code is what a train trip search uses.
/// </summary>
public sealed record StationSuggestion(int StationId, string Code, string Name, string City);
