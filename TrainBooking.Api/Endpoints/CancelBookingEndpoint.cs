using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Bookings;
using TrainBooking.Application.Bookings.Commands.CancelBooking;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Cancels one of the calling customer's unpaid bookings before its seat hold runs out.
/// </summary>
public class CancelBookingEndpoint : EndpointWithoutRequest<BookingDetailResponse>
{
    private readonly ISender _sender;

    public CancelBookingEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/bookings/{code}/cancel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new CancelBookingCommand(CustomerHeader.Read(HttpContext), Route<string>("code")!);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
