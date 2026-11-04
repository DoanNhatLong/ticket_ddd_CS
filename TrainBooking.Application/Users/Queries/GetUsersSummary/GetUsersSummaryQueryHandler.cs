using MediatR;
using TrainBooking.Domain.Users;

namespace TrainBooking.Application.Users.Queries.GetUsersSummary;

public sealed class GetUsersSummaryQueryHandler
    : IRequestHandler<GetUsersSummaryQuery, IReadOnlyList<UserSummaryResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersSummaryQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> Handle(
        GetUsersSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Select(u => new UserSummaryResponse(u.FullName, u.Role.ToString()))
            .ToList();
    }
}
