using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Search;

public static class TrainTripSearchErrors
{
    public static readonly Error InvalidDate = Error.Validation(
        "TrainTripSearch.InvalidDate",
        "The date must look like 2027-01-31.");

    public static readonly Error TextTooLong = Error.Validation(
        "TrainTripSearch.TextTooLong",
        "The search text is too long.");

    public static readonly Error InvalidPage = Error.Validation(
        "TrainTripSearch.InvalidPage",
        "The page must be at least 1 and the page size between 1 and 50; only the first 10000 results can be read.");
}
