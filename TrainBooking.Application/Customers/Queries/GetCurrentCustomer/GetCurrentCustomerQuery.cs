using MediatR;
using TrainBooking.Application.Abstractions;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Customers.Queries.GetCurrentCustomer;

/// <summary>
/// Returns the customer identified by the request.
/// </summary>
public sealed record GetCurrentCustomerQuery(int? CustomerId)
    : IRequest<Result<CurrentCustomerResponse>>, ICustomerRequest;
