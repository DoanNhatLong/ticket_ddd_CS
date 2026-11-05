namespace TrainBooking.Application.Abstractions;

/// <summary>
/// Marks a request that must be made by a known customer.
/// <see cref="Behaviors.CustomerAuthenticationBehavior{TRequest,TResponse}"/> rejects it before the handler runs
/// when the customer is missing or unknown. Such requests must return a <c>Result</c>.
/// </summary>
public interface ICustomerRequest
{
    /// <summary>
    /// Id of the customer making the request, or <c>null</c> when the caller did not identify themselves.
    /// </summary>
    int? CustomerId { get; }
}
