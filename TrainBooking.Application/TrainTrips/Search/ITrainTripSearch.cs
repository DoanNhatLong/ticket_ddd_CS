using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Search;

/// <summary>
/// Searches train trips by text, stations and departure date.
/// </summary>
public interface ITrainTripSearch
{
    /// <summary>
    /// Returns one page of train trips. No match is a success with no items; it fails when the
    /// search engine cannot be reached.
    /// </summary>
    Task<Result<TrainTripSearchPage>> SearchAsync(
        TrainTripSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds the search data from the train trips in the database and returns how many were indexed.
    /// </summary>
    Task<Result<int>> RebuildIndexAsync(CancellationToken cancellationToken = default);
}
