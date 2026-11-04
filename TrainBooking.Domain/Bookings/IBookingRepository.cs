namespace TrainBooking.Domain.Bookings;

public interface IBookingRepository
{
    /// <summary>
    /// Gets all bookings ordered by Status, then by CreatedAt (both ascending).
    /// </summary>
    Task<IReadOnlyList<Booking>> GetAllOrderedByStatusAndCreatedAtAsync(
        CancellationToken cancellationToken = default);
}
