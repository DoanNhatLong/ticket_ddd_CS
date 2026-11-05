namespace TrainBooking.Application.Customers.Queries.GetCurrentCustomer;

public sealed record CurrentCustomerResponse(int Id, string FullName, string Email);
