using MediatR;
using TrainBooking.Application.TrainTrips.Search;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Queries.SearchTrainTrips;

public sealed class SearchTrainTripsQueryHandler
    : IRequestHandler<SearchTrainTripsQuery, Result<TrainTripSearchPage>>
{
    private const int MaxTextLength = 100;
    private const int MaxPageSize = 50;

    // Search engines refuse to page deeper than this.
    private const int MaxReachableResults = 10_000;

    private readonly ITrainTripSearch _trainTripSearch;
    private readonly TimeProvider _clock;

    public SearchTrainTripsQueryHandler(ITrainTripSearch trainTripSearch, TimeProvider clock)
    {
        _trainTripSearch = trainTripSearch;
        _clock = clock;
    }

    public async Task<Result<TrainTripSearchPage>> Handle(
        SearchTrainTripsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1
            || request.PageSize is < 1 or > MaxPageSize
            || (long)request.Page * request.PageSize > MaxReachableResults)
        {
            return TrainTripSearchErrors.InvalidPage;
        }

        var text = NullIfBlank(request.Text);
        if (text?.Length > MaxTextLength)
            return TrainTripSearchErrors.TextTooLong;

        // A train trip that has departed cannot be booked any more.
        var criteria = new TrainTripSearchCriteria(
            text,
            NullIfBlank(request.DepartureStationCode)?.ToUpperInvariant(),
            NullIfBlank(request.ArrivalStationCode)?.ToUpperInvariant(),
            request.DepartureDate,
            _clock.GetUtcNow().UtcDateTime,
            request.Page,
            request.PageSize);

        return await _trainTripSearch.SearchAsync(criteria, cancellationToken);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
