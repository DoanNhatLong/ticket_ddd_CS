namespace TrainBooking.Domain.TrainTrips;

public interface ITrainTripRepository
{
    /// <summary>
    /// Gets a train trip with its fares, or <c>null</c> when it does not exist.
    /// </summary>
    Task<TrainTrip?> GetByIdAsync(int trainTripId, CancellationToken cancellationToken = default);
}
