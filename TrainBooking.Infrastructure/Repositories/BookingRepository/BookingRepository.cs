using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.Bookings;
using TrainBooking.Infrastructure.Entity;
using BookingRecord = TrainBooking.Infrastructure.Entity.Booking;
using DomainBooking = TrainBooking.Domain.Bookings.Booking;

namespace TrainBooking.Infrastructure.Repositories.BookingRepository;

/// <summary>
/// EF Core implementation of <see cref="IBookingRepository"/>.
/// Reads the scaffolded persistence model and maps it to the domain aggregate.
/// </summary>
internal sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DomainBooking>> GetAllOrderedByStatusAndCreatedAtAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await _context.Bookings
            .AsNoTracking()
            .OrderBy(b => b.Status)
            .ThenBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlySet<int>> GetTakenSeatIdsAsync(
        int trainTripId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var paid = new[] { nameof(BookingStatus.Confirmed), nameof(BookingStatus.Completed) };
        var pending = nameof(BookingStatus.Pending);

        var seatIds = await _context.Bookingitems
            .AsNoTracking()
            .Where(item => item.TrainTripId == trainTripId)
            .Where(item => paid.Contains(item.Booking.Status)
                || (item.Booking.Status == pending && item.Booking.ExpiresAt > nowUtc))
            .Select(item => item.SeatId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return seatIds.ToHashSet();
    }

    private static DomainBooking ToDomain(BookingRecord record)
    {
        return DomainBooking.Rehydrate(
            record.Id,
            record.UserId,
            record.BookingCode,
            record.TotalAmount,
            ParseStatus(record.Status),
            record.CreatedAt,
            record.ExpiresAt);
    }

    private static BookingStatus ParseStatus(string status)
    {
        if (Enum.TryParse<BookingStatus>(status, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown booking status '{status}' found in database.");
    }
}
