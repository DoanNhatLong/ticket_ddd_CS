namespace TrainBooking.Api.Tests.Infrastructure;

/// <summary>
/// The application's clock in tests. Unlike FakeTimeProvider it can be set backwards, so every test
/// can start from <see cref="Start"/> while sharing one application instance.
/// </summary>
public sealed class TestClock : TimeProvider
{
    public static readonly DateTimeOffset Start = new(2026, 11, 1, 8, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _utcNow = Start;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow.ToUniversalTime();

    public void Advance(TimeSpan delta) => _utcNow += delta;

    public void Reset() => _utcNow = Start;
}
