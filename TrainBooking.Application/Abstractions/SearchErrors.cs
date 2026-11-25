using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Abstractions;

public static class SearchErrors
{
    public static readonly Error Unavailable = Error.Unavailable(
        "Search.Unavailable",
        "Search is temporarily unavailable. Please try again later.");
}
