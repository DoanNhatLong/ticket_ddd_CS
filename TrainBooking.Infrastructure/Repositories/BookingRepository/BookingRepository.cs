using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;
using TrainBooking.Infrastructure.Entity;
using BookingRecord = TrainBooking.Infrastructure.Entity.Booking;
using DomainBooking = TrainBooking.Domain.Bookings.Booking;
using DomainTicket = TrainBooking.Domain.Bookings.Ticket;
using TicketRecord = TrainBooking.Infrastructure.Entity.Ticket;

namespace TrainBooking.Infrastructure.Repositories.BookingRepository;

/// <summary>
/// EF Core implementation of <see cref="IBookingRepository"/>.
/// Maps the Booking aggregate (booking + tickets) to the scaffolded persistence model and owns the
/// double-booking guarantee: the unique index on the generated Tickets.OccupiedSeatKey column.
/// </summary>
internal sealed class BookingRepository : IBookingRepository
{
    private const string OccupiedSeatIndexName = "UQ_Tickets_OccupiedSeatKey";

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

    public async Task<DomainBooking?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default)
    {
        var record = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Tickets.OrderBy(t => t.Id))
            .SingleOrDefaultAsync(b => b.BookingCode == bookingCode, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    public async Task<Result> AddAsync(
        DomainBooking booking,
        IReadOnlyCollection<DomainBooking> expiredBookings,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Release the expired holds first, in their own statements, so the unique occupied-seat
            // index no longer sees them when the new tickets are inserted.
            if (expiredBookings.Count > 0)
            {
                await ApplyStatusChangesAsync(expiredBookings, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _context.Bookings.Add(ToRecord(booking));
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException exception) when (IsOccupiedSeatViolation(exception))
        {
            // Another booking holds or owns one of the seats. Roll back (the expiries too, which the
            // background expiry will redo) and forget the failed changes so the scoped context stays
            // usable for the rest of the request.
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            return BookingErrors.SeatTaken;
        }
    }

    public async Task UpdateAsync(DomainBooking booking, CancellationToken cancellationToken = default)
    {
        await ApplyStatusChangesAsync([booking], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DomainBooking>> GetOverdueHoldingSeatsAsync(
        IReadOnlyCollection<(int TrainTripId, int SeatId)> seats,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (seats.Count == 0)
            return [];

        // Same format as the generated Tickets.OccupiedSeatKey column.
        var seatKeys = seats.Select(s => $"{s.TrainTripId}:{s.SeatId}").ToList();
        var pending = nameof(BookingStatus.Pending);

        var records = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Tickets.OrderBy(t => t.Id))
            .Where(b => b.Status == pending && b.ExpiresAt <= nowUtc)
            .Where(b => b.Tickets.Any(t => t.OccupiedSeatKey != null && seatKeys.Contains(t.OccupiedSeatKey)))
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<DomainBooking>> GetOverdueAsync(
        DateTime nowUtc,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        var pending = nameof(BookingStatus.Pending);

        var records = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Tickets.OrderBy(t => t.Id))
            .Where(b => b.Status == pending && b.ExpiresAt <= nowUtc)
            .OrderBy(b => b.ExpiresAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlySet<int>> GetTakenSeatIdsAsync(
        int trainTripId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var active = nameof(TicketStatus.Active);
        var held = nameof(TicketStatus.Held);

        var seatIds = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.TrainTripId == trainTripId)
            .Where(t => t.Status == active || (t.Status == held && t.Booking.ExpiresAt > nowUtc))
            .Select(t => t.SeatId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return seatIds.ToHashSet();
    }

    /// <summary>
    /// Copies the booking and ticket statuses of <paramref name="bookings"/> onto tracked records.
    /// Statuses are the only state that changes after a booking is created.
    /// </summary>
    private async Task ApplyStatusChangesAsync(
        IReadOnlyCollection<DomainBooking> bookings,
        CancellationToken cancellationToken)
    {
        var ids = bookings.Select(b => b.Id).ToList();
        var records = await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => ids.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, cancellationToken);

        foreach (var booking in bookings)
        {
            if (!records.TryGetValue(booking.Id, out var record))
                throw new InvalidOperationException($"Booking {booking.Id} does not exist.");

            record.Status = booking.Status.ToString();

            var ticketRecords = record.Tickets.ToDictionary(t => t.Id);
            foreach (var ticket in booking.Tickets)
                ticketRecords[ticket.Id].Status = ticket.Status.ToString();
        }
    }

    private static bool IsOccupiedSeatViolation(DbUpdateException exception)
    {
        return exception.InnerException is MySqlException
        {
            ErrorCode: MySqlErrorCode.DuplicateKeyEntry
        } mySqlException
            && mySqlException.Message.Contains(OccupiedSeatIndexName, StringComparison.Ordinal);
    }

    private static BookingRecord ToRecord(DomainBooking booking)
    {
        return new BookingRecord
        {
            UserId = booking.UserId,
            BookingCode = booking.BookingCode,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ExpiresAt = booking.ExpiresAt,
            Tickets = booking.Tickets
                .Select(t => new TicketRecord
                {
                    TrainTripId = t.TrainTripId,
                    SeatId = t.SeatId,
                    PassengerName = t.PassengerName,
                    PassengerIdNumber = t.PassengerIdNumber,
                    Price = t.Price,
                    Status = t.Status.ToString()
                })
                .ToList()
        };
    }

    private static DomainBooking ToDomain(BookingRecord record)
    {
        return DomainBooking.Rehydrate(
            record.Id,
            record.UserId,
            record.BookingCode,
            record.TotalAmount,
            ParseEnum<BookingStatus>(record.Status, "booking status"),
            record.CreatedAt is { } createdAt ? DateTime.SpecifyKind(createdAt, DateTimeKind.Utc) : null,
            DateTime.SpecifyKind(record.ExpiresAt, DateTimeKind.Utc),
            record.Tickets.Select(ToDomain));
    }

    private static DomainTicket ToDomain(TicketRecord record)
    {
        return DomainTicket.Rehydrate(
            record.Id,
            record.TrainTripId,
            record.SeatId,
            record.PassengerName,
            record.PassengerIdNumber,
            record.Price,
            ParseEnum<TicketStatus>(record.Status, "ticket status"));
    }

    private static TEnum ParseEnum<TEnum>(string value, string description)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown {description} '{value}' found in database.");
    }
}
