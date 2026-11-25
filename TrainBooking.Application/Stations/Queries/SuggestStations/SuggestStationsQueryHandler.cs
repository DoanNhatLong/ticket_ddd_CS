using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Stations.Queries.SuggestStations;

public sealed class SuggestStationsQueryHandler
    : IRequestHandler<SuggestStationsQuery, Result<IReadOnlyList<StationSuggestion>>>
{
    private const int MaxTextLength = 50;
    private const int MaxAllowedCount = 20;

    private readonly IStationSuggester _stationSuggester;

    public SuggestStationsQueryHandler(IStationSuggester stationSuggester)
    {
        _stationSuggester = stationSuggester;
    }

    public async Task<Result<IReadOnlyList<StationSuggestion>>> Handle(
        SuggestStationsQuery request,
        CancellationToken cancellationToken)
    {
        var text = request.Text?.Trim() ?? string.Empty;

        // Nothing typed yet, or longer than any station name: no suggestion, no call to the engine.
        if (text.Length is 0 or > MaxTextLength)
            return Result.Success<IReadOnlyList<StationSuggestion>>([]);

        var maxCount = Math.Clamp(request.MaxCount, 1, MaxAllowedCount);

        return await _stationSuggester.SuggestAsync(text, maxCount, cancellationToken);
    }
}
