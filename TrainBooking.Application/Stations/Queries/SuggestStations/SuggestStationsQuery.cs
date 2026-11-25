using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Stations.Queries.SuggestStations;

/// <summary>
/// Suggests stations for the text the customer has typed so far.
/// </summary>
public sealed record SuggestStationsQuery(
    string? Text,
    int MaxCount = SuggestStationsQuery.DefaultMaxCount)
    : IRequest<Result<IReadOnlyList<StationSuggestion>>>
{
    public const int DefaultMaxCount = 5;
}
