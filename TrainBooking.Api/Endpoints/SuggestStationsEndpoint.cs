using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Stations;
using TrainBooking.Application.Stations.Queries.SuggestStations;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Suggests stations for the text typed so far: <c>q</c> is the text, <c>max</c> the number of suggestions.
/// </summary>
public class SuggestStationsEndpoint : EndpointWithoutRequest<IReadOnlyList<StationSuggestion>>
{
    private readonly ISender _sender;

    public SuggestStationsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/stations/suggest");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var text = Query<string?>("q", isRequired: false);
        var maxCount = Query<int?>("max", isRequired: false) ?? SuggestStationsQuery.DefaultMaxCount;

        var result = await _sender.Send(new SuggestStationsQuery(text, maxCount), ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
