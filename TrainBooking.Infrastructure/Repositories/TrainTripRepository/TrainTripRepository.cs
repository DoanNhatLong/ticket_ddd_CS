using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.TrainTrips;
using TrainBooking.Infrastructure.Entity;
using DomainTrainTrip = TrainBooking.Domain.TrainTrips.TrainTrip;
using TrainTripRecord = TrainBooking.Infrastructure.Entity.Traintrip;

namespace TrainBooking.Infrastructure.Repositories.TrainTripRepository;

/// <summary>
/// EF Core implementation of <see cref="ITrainTripRepository"/>.
/// </summary>
internal sealed class TrainTripRepository : ITrainTripRepository
{
    private readonly AppDbContext _context;

    public TrainTripRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DomainTrainTrip?> GetByIdAsync(int trainTripId, CancellationToken cancellationToken = default)
    {
        var record = await _context.Traintrips
            .AsNoTracking()
            .Include(t => t.Fares)
            .SingleOrDefaultAsync(t => t.Id == trainTripId, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    private static DomainTrainTrip ToDomain(TrainTripRecord record)
    {
        return DomainTrainTrip.Rehydrate(
            record.Id,
            record.TrainId,
            DateTime.SpecifyKind(record.DepartureTime, DateTimeKind.Utc),
            DateTime.SpecifyKind(record.ArrivalTime, DateTimeKind.Utc),
            ParseStatus(record.Status),
            record.Fares.Select(f => new Domain.TrainTrips.Fare(f.SeatClassId, f.Price)));
    }

    private static TrainTripStatus ParseStatus(string status)
    {
        if (Enum.TryParse<TrainTripStatus>(status, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown train trip status '{status}' found in database.");
    }
}
