using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Infrastructure.Repositories.SeatRepository;

/// <summary>
/// Decorator over <see cref="ISeatRepository"/> that caches the seat layout of each train in Redis.
/// A train's seats (carriages, seat numbers, seat classes) are configuration data that rarely changes
/// and is shared by every trip of that train, so it is safe to serve slightly stale copies.
/// Seat availability is NOT cached here: it comes from bookings and must always be fresh.
/// The cache is fail-open: when Redis is unavailable, reads go straight to the database.
/// </summary>
internal sealed class CachedSeatRepository : ISeatRepository
{
    // Bump the version when the cached shape changes so old entries are ignored.
    private const string KeyPrefix = "seats:train:v1:";

    private static readonly DistributedCacheEntryOptions EntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
    };

    private readonly ISeatRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedSeatRepository> _logger;

    public CachedSeatRepository(
        ISeatRepository inner,
        IDistributedCache cache,
        ILogger<CachedSeatRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Seat>> GetByTrainAsync(int trainId, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + trainId;

        var cached = await TryReadAsync(key, cancellationToken);
        if (cached is not null)
            return cached;

        var seats = await _inner.GetByTrainAsync(trainId, cancellationToken);

        // An empty layout usually means the train is not set up yet; do not pin that for hours.
        if (seats.Count > 0)
            await TryWriteAsync(key, seats, cancellationToken);

        return seats;
    }

    public Task<IReadOnlyList<Seat>> GetByIdsAsync(
        IReadOnlyCollection<int> seatIds,
        CancellationToken cancellationToken = default)
    {
        // Used by the booking flow; always read from the database.
        return _inner.GetByIdsAsync(seatIds, cancellationToken);
    }

    private async Task<IReadOnlyList<Seat>?> TryReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken);
            if (bytes is null)
                return null;

            var snapshots = JsonSerializer.Deserialize<List<SeatSnapshot>>(bytes);
            return snapshots?.Select(s => s.ToSeat()).ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Reading seat layout from cache failed for key {CacheKey}; falling back to the database.", key);
            return null;
        }
    }

    private async Task TryWriteAsync(string key, IReadOnlyList<Seat> seats, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(seats.Select(SeatSnapshot.From).ToList());
            await _cache.SetAsync(key, bytes, EntryOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Writing seat layout to cache failed for key {CacheKey}.", key);
        }
    }

    /// <summary>
    /// Flat, serializable copy of a seat; domain entities are not serialized directly.
    /// </summary>
    internal sealed record SeatSnapshot(
        int Id,
        int TrainId,
        int CarriageNumber,
        string SeatNumber,
        int SeatClassId,
        string SeatClassCode,
        string SeatClassName)
    {
        public static SeatSnapshot From(Seat seat) => new(
            seat.Id,
            seat.TrainId,
            seat.CarriageNumber,
            seat.SeatNumber,
            seat.SeatClass.Id,
            seat.SeatClass.Code,
            seat.SeatClass.Name);

        public Seat ToSeat() => Seat.Rehydrate(
            Id,
            TrainId,
            CarriageNumber,
            SeatNumber,
            new SeatClass(SeatClassId, SeatClassCode, SeatClassName));
    }
}
