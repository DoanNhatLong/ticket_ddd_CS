namespace TrainBooking.Domain.Bookings;

public interface IBookingRepository
{
    /// <summary>
    /// Gets all bookings ordered by Status, then by CreatedAt (both ascending).
    /// </summary>
    Task<IReadOnlyList<Booking>> GetAllOrderedByStatusAndCreatedAtAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ids of seats taken on a train trip at <paramref name="nowUtc"/>: seats on tickets of
    /// paid bookings, and of unpaid bookings whose seat hold has not run out.
    /// </summary>
    Task<IReadOnlySet<int>> GetTakenSeatIdsAsync(
        int trainTripId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
