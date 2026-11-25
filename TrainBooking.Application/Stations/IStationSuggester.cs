using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Stations;

/// <summary>
/// Suggests stations while the customer is still typing, tolerating missing accents and small typos.
/// </summary>
public interface IStationSuggester
{
    /// <summary>
    /// Suggests up to <paramref name="maxCount"/> stations for the text typed so far, best match first.
    /// No match is a success with an empty list; it fails when the suggestion engine cannot be reached.
    /// </summary>
    Task<Result<IReadOnlyList<StationSuggestion>>> SuggestAsync(
        string text,
        int maxCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds the suggestions from the stations in the database and returns how many were indexed.
    /// </summary>
    Task<Result<int>> RebuildIndexAsync(CancellationToken cancellationToken = default);
}
