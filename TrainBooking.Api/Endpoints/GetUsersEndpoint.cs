using FastEndpoints;
using TrainBooking.Application.Dtos;
using TrainBooking.Application.Interfaces;

namespace TrainBooking.Api.Endpoints;

public class GetUsersEndpoint : EndpointWithoutRequest<IEnumerable<UserSummaryResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersEndpoint(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override void Configure()
    {
        Get("/api/users/summary");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _userRepository.GetUsersSummaryAsync();
        await SendAsync(users, cancellation: ct);
    }
}
