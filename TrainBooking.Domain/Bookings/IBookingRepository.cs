using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Bookings;

public interface IBookingRepository
{
    /// <summary>
    /// Gets all bookings (without tickets) ordered by Status, then by CreatedAt (both ascending).
    /// </summary>
    Task<IReadOnlyList<Booking>> GetAllOrderedByStatusAndCreatedAtAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a booking with its tickets by booking code, or <c>null</c> when it does not exist.
    /// </summary>
    Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new booking with its tickets. Fails with <see cref="BookingErrors.SeatTaken"/> when any
    /// ticket's seat is already held or sold on its train trip; nothing is saved in that case.
    /// </summary>
    Task<Result> AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ids of seats taken on a train trip at <paramref name="nowUtc"/>: seats on active tickets,
    /// and on held tickets whose booking's seat hold has not run out.
    /// </summary>
    Task<IReadOnlySet<int>> GetTakenSeatIdsAsync(
        int trainTripId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
