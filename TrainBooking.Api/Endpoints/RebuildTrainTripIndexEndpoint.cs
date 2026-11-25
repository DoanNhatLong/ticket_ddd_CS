using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.TrainTrips.Commands.RebuildTrainTripIndex;

namespace TrainBooking.Api.Endpoints;

public sealed record RebuildTrainTripIndexResponse(int IndexedTrainTrips);

/// <summary>
/// Rebuilds the train trip search data, e.g. after train trips in the database were changed by hand.
/// Open to everyone until real authentication exists.
/// </summary>
public class RebuildTrainTripIndexEndpoint : EndpointWithoutRequest<RebuildTrainTripIndexResponse>
{
    private readonly ISender _sender;

    public RebuildTrainTripIndexEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/admin/search/train-trips/rebuild");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new RebuildTrainTripIndexCommand(), ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(new RebuildTrainTripIndexResponse(result.Value), ct);
    }
}
