using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Users;

namespace TrainBooking.Application.Customers.Queries.GetCurrentCustomer;

public sealed class GetCurrentCustomerQueryHandler
    : IRequestHandler<GetCurrentCustomerQuery, Result<CurrentCustomerResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentCustomerQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<CurrentCustomerResponse>> Handle(
        GetCurrentCustomerQuery request,
        CancellationToken cancellationToken)
    {
        // CustomerAuthenticationBehavior has already rejected missing or unknown customers.
        var user = await _userRepository.GetByIdAsync(request.CustomerId!.Value, cancellationToken);
        if (user is null)
            return CustomerErrors.Unknown;

        return new CurrentCustomerResponse(user.Id, user.FullName, user.Email);
    }
}
