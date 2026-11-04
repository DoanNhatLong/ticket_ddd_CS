using FastEndpoints;
using MediatR;
using TrainBooking.Application.Users.Queries.GetUsersSummary;

namespace TrainBooking.Api.Endpoints;

public class GetUsersEndpoint : EndpointWithoutRequest<IReadOnlyList<UserSummaryResponse>>
{
    private readonly ISender _sender;

    public GetUsersEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/users/summary");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _sender.Send(new GetUsersSummaryQuery(), ct);
        await SendOkAsync(users, ct);
    }
}
