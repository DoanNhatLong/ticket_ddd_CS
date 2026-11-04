using FastEndpoints;
using MediatR;
using TrainBooking.Application.Bookings.Queries.GetAllBookings;

namespace TrainBooking.Api.Endpoints;

public class GetAllBookingsEndpoint : EndpointWithoutRequest<IReadOnlyList<BookingResponse>>
{
    private readonly ISender _sender;

    public GetAllBookingsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/bookings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bookings = await _sender.Send(new GetAllBookingsQuery(), ct);
        await SendOkAsync(bookings, ct);
    }
}
