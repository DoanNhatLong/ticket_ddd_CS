namespace TrainBooking.Domain.Trains;

public interface ISeatRepository
{
    /// <summary>
    /// Gets every seat of a train, ordered by carriage number then seat number.
    /// </summary>
    Task<IReadOnlyList<Seat>> GetByTrainAsync(int trainId, CancellationToken cancellationToken = default);
}
