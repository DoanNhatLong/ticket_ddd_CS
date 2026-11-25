using MediatR;
using TrainBooking.Application.TrainTrips.Search;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Queries.SearchTrainTrips;

/// <summary>
/// Searches the train trips that have not departed yet.
/// </summary>
public sealed record SearchTrainTripsQuery(
    string? Text,
    string? DepartureStationCode,
    string? ArrivalStationCode,
    DateOnly? DepartureDate,
    int Page = SearchTrainTripsQuery.DefaultPage,
    int PageSize = SearchTrainTripsQuery.DefaultPageSize)
    : IRequest<Result<TrainTripSearchPage>>
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
}
