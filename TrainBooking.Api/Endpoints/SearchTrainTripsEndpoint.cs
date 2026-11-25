using System.Globalization;
using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.TrainTrips.Queries.SearchTrainTrips;
using TrainBooking.Application.TrainTrips.Search;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Searches train trips that have not departed yet: <c>q</c> free text, <c>from</c> and <c>to</c> station codes,
/// <c>date</c> departure day (yyyy-MM-dd, UTC), <c>page</c> and <c>pageSize</c>. All are optional.
/// </summary>
public class SearchTrainTripsEndpoint : EndpointWithoutRequest<TrainTripSearchPage>
{
    private readonly ISender _sender;

    public SearchTrainTripsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/train-trips/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!TryReadDate(Query<string?>("date", isRequired: false), out var date))
        {
            await HttpContext.SendErrorAsync(TrainTripSearchErrors.InvalidDate, ct);
            return;
        }

        var query = new SearchTrainTripsQuery(
            Query<string?>("q", isRequired: false),
            Query<string?>("from", isRequired: false),
            Query<string?>("to", isRequired: false),
            date,
            Query<int?>("page", isRequired: false) ?? SearchTrainTripsQuery.DefaultPage,
            Query<int?>("pageSize", isRequired: false) ?? SearchTrainTripsQuery.DefaultPageSize);

        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }

    private static bool TryReadDate(string? text, out DateOnly? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (!DateOnly.TryParseExact(
                text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return false;
        }

        date = parsed;
        return true;
    }
}
