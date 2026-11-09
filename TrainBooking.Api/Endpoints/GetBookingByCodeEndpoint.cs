using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Bookings;
using TrainBooking.Application.Bookings.Queries.GetBookingByCode;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Returns one of the calling customer's bookings by its booking code.
/// </summary>
public class GetBookingByCodeEndpoint : EndpointWithoutRequest<BookingDetailResponse>
{
    private readonly ISender _sender;

    public GetBookingByCodeEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/bookings/{code}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetBookingByCodeQuery(CustomerHeader.Read(HttpContext), Route<string>("code")!);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
