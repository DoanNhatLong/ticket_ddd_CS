using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.TrainTrips;

public static class TrainTripErrors
{
    public static Error NotFound(int trainTripId) => Error.NotFound(
        "TrainTrip.NotFound",
        $"Train trip {trainTripId} does not exist.");
}
