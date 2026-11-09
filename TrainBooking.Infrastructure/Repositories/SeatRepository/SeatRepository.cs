using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.Trains;
using TrainBooking.Infrastructure.Entity;
using DomainSeat = TrainBooking.Domain.Trains.Seat;
using DomainSeatClass = TrainBooking.Domain.Trains.SeatClass;
using SeatRecord = TrainBooking.Infrastructure.Entity.Seat;

namespace TrainBooking.Infrastructure.Repositories.SeatRepository;

/// <summary>
/// EF Core implementation of <see cref="ISeatRepository"/>.
/// </summary>
internal sealed class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyList<DomainSeat>> GetByTrainAsync(int trainId, CancellationToken cancellationToken = default)
    {
        return QueryAsync(_context.Seats.Where(s => s.Carriage.TrainId == trainId), cancellationToken);
    }

    public Task<IReadOnlyList<DomainSeat>> GetByIdsAsync(
        IReadOnlyCollection<int> seatIds,
        CancellationToken cancellationToken = default)
    {
        var ids = seatIds.ToList();
        return QueryAsync(_context.Seats.Where(s => ids.Contains(s.Id)), cancellationToken);
    }

    private static async Task<IReadOnlyList<DomainSeat>> QueryAsync(
        IQueryable<SeatRecord> seats,
        CancellationToken cancellationToken)
    {
        var records = await seats
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Carriage.TrainId,
                s.Carriage.CarriageNumber,
                s.SeatNumber,
                SeatClassId = s.SeatClass.Id,
                SeatClassCode = s.SeatClass.Code,
                SeatClassName = s.SeatClass.Name
            })
            .OrderBy(s => s.CarriageNumber)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);

        return records
            .Select(r => DomainSeat.Rehydrate(
                r.Id,
                r.TrainId,
                r.CarriageNumber,
                r.SeatNumber,
                new DomainSeatClass(r.SeatClassId, r.SeatClassCode, r.SeatClassName)))
            .ToList();
    }
}
