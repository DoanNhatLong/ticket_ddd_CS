using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Stations.Commands.RebuildStationIndex;

namespace TrainBooking.Api.Endpoints;

public sealed record RebuildStationIndexResponse(int IndexedStations);

/// <summary>
/// Rebuilds the station suggestions, e.g. after the stations in the database were changed by hand.
/// Open to everyone until real authentication exists.
/// </summary>
public class RebuildStationIndexEndpoint : EndpointWithoutRequest<RebuildStationIndexResponse>
{
    private readonly ISender _sender;

    public RebuildStationIndexEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/admin/search/stations/rebuild");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new RebuildStationIndexCommand(), ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(new RebuildStationIndexResponse(result.Value), ct);
    }
}
