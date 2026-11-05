using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.Trains;
using TrainBooking.Infrastructure.Entity;
using DomainSeat = TrainBooking.Domain.Trains.Seat;
using DomainSeatClass = TrainBooking.Domain.Trains.SeatClass;

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

    public async Task<IReadOnlyList<DomainSeat>> GetByTrainAsync(int trainId, CancellationToken cancellationToken = default)
    {
        var records = await _context.Seats
            .AsNoTracking()
            .Where(s => s.Carriage.TrainId == trainId)
            .Select(s => new
            {
                s.Id,
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
                r.CarriageNumber,
                r.SeatNumber,
                new DomainSeatClass(r.SeatClassId, r.SeatClassCode, r.SeatClassName)))
            .ToList();
    }
}
