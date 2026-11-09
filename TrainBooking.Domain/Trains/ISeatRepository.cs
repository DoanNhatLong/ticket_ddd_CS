namespace TrainBooking.Domain.Trains;

public interface ISeatRepository
{
    /// <summary>
    /// Gets every seat of a train, ordered by carriage number then seat number.
    /// </summary>
    Task<IReadOnlyList<Seat>> GetByTrainAsync(int trainId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the seats with the given ids; ids that do not exist are left out.
    /// </summary>
    Task<IReadOnlyList<Seat>> GetByIdsAsync(IReadOnlyCollection<int> seatIds, CancellationToken cancellationToken = default);
}
