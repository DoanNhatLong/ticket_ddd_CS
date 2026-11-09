using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Bookings;
using TrainBooking.Application.Bookings.Commands.CreateBooking;

namespace TrainBooking.Api.Endpoints;

public sealed record CreateBookingRequest(IReadOnlyList<CreateBookingTicket>? Tickets);

/// <summary>
/// Creates a pending booking for the customer in the <c>X-Customer-Id</c> header and holds its seats.
/// </summary>
public class CreateBookingEndpoint : Endpoint<CreateBookingRequest, BookingDetailResponse>
{
    private readonly ISender _sender;

    public CreateBookingEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/bookings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var command = new CreateBookingCommand(CustomerHeader.Read(HttpContext), request.Tickets ?? []);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendAsync(result.Value, StatusCodes.Status201Created, ct);
    }
}
