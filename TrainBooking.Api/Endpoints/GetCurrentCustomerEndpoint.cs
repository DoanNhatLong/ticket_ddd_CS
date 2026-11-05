using FastEndpoints;
using MediatR;
using TrainBooking.Api.Http;
using TrainBooking.Application.Customers.Queries.GetCurrentCustomer;

namespace TrainBooking.Api.Endpoints;

/// <summary>
/// Returns the customer identified by the <c>X-Customer-Id</c> header.
/// </summary>
public class GetCurrentCustomerEndpoint : EndpointWithoutRequest<CurrentCustomerResponse>
{
    private readonly ISender _sender;

    public GetCurrentCustomerEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/customers/me");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCurrentCustomerQuery(CustomerHeader.Read(HttpContext)), ct);

        if (result.IsFailure)
        {
            await HttpContext.SendErrorAsync(result.Error, ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
