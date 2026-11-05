using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.TrainTrips.Queries.GetTripSeats;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Lists every seat on a train trip with its seat class, fare and whether it is free.
/// </summary>
public class GetTripSeatsEndpoint : EndpointWithoutRequest<TripSeatsResponse>
{
    private readonly ISender _sender;

    public GetTripSeatsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/train-trips/{tripId:int}/seats");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tripId = Route<int>("tripId");
        var result = await _sender.Send(new GetTripSeatsQuery(tripId), ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
