using MediatR;
using TrainBooking.Application.Customers;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Users;

namespace TrainBooking.Application.Abstractions.Behaviors;

/// <summary>
/// Rejects <see cref="ICustomerRequest"/>s whose customer is missing, unknown, or not a customer,
/// returning an Unauthorized result instead of calling the handler.
/// </summary>
public sealed class CustomerAuthenticationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUserRepository _userRepository;

    public CustomerAuthenticationBehavior(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICustomerRequest customerRequest)
            return await next();

        if (customerRequest.CustomerId is not { } customerId)
            return ResultFailureFactory.Create<TResponse>(CustomerErrors.Missing);

        var user = await _userRepository.GetByIdAsync(customerId, cancellationToken);
        if (user is null || user.Role != UserRole.Customer)
            return ResultFailureFactory.Create<TResponse>(CustomerErrors.Unknown);

        return await next();
    }
}
