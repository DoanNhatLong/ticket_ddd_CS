using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Customers;

public static class CustomerErrors
{
    public static readonly Error Missing = Error.Unauthorized(
        "Customer.Missing",
        "The request does not identify a customer.");

    public static readonly Error Unknown = Error.Unauthorized(
        "Customer.Unknown",
        "The request identifies a customer that does not exist.");
}
