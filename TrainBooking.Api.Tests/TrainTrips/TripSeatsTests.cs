using System.Net;
using System.Text.Json;
using TrainBooking.Api.Tests.Infrastructure;

namespace TrainBooking.Api.Tests.TrainTrips;

/// <summary>
/// Seat map of a train trip. Seed data (init.sql): train trip 1 runs train 1, whose seats are
/// A01 (hard berth lower, 1,200,000), A02 (hard berth middle, no fare), B01 and B02 (soft seat, 750,000).
/// Unpaid bookings 1 and 5 hold A01, B01 and A02 on trip 1 until 2026-11-03 18:00 / 22:00 UTC.
/// </summary>
public sealed class TripSeatsTests : ApiTestBase
{
    private const int TripId = 1;
    private const int ConfirmedBookingId = 2;
    private const int CancelledBookingId = 4;
    private const int SeatB02 = 4;

    private static readonly DateTimeOffset AfterAllSeededHoldsRanOut = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    public TripSeatsTests(TrainBookingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Lists_every_seat_of_the_trip_with_carriage_seat_class_and_fare()
    {
        var seats = await GetSeatsAsync(TripId);

        Assert.Equal(["A01", "A02", "B01", "B02"], seats.Select(s => s.GetProperty("seatNumber").GetString()));

        var a01 = Seat(seats, "A01");
        Assert.Equal(1, a01.GetProperty("carriageNumber").GetInt32());
        Assert.Equal("HARD_BERTH_LOWER", a01.GetProperty("seatClassCode").GetString());
        Assert.Equal(1_200_000m, a01.GetProperty("fare").GetDecimal());

        var b01 = Seat(seats, "B01");
        Assert.Equal(2, b01.GetProperty("carriageNumber").GetInt32());
        Assert.Equal("SOFT_SEAT", b01.GetProperty("seatClassCode").GetString());
        Assert.Equal(750_000m, b01.GetProperty("fare").GetDecimal());
    }

    [Fact]
    public async Task Seat_class_without_a_fare_on_the_trip_has_no_fare()
    {
        var seats = await GetSeatsAsync(TripId);

        Assert.Equal(JsonValueKind.Null, Seat(seats, "A02").GetProperty("fare").ValueKind);
    }

    [Fact]
    public async Task Seats_of_the_same_seat_class_cost_the_same_whatever_the_carriage_type()
    {
        await Factory.Database.ExecuteAsync("UPDATE Carriages SET Type = 'Luxury' WHERE Id = 2");

        var seats = await GetSeatsAsync(TripId);

        Assert.Equal(750_000m, Seat(seats, "B01").GetProperty("fare").GetDecimal());
        Assert.Equal(750_000m, Seat(seats, "B02").GetProperty("fare").GetDecimal());
    }

    [Fact]
    public async Task Seats_held_by_an_unpaid_booking_are_taken_and_others_are_free()
    {
        var seats = await GetSeatsAsync(TripId);

        Assert.False(IsFree(seats, "A01"));
        Assert.False(IsFree(seats, "A02"));
        Assert.False(IsFree(seats, "B01"));
        Assert.True(IsFree(seats, "B02"));
    }

    [Fact]
    public async Task Seats_become_free_once_the_seat_hold_runs_out()
    {
        Factory.Clock.SetUtcNow(AfterAllSeededHoldsRanOut);

        var seats = await GetSeatsAsync(TripId);

        Assert.All(seats, seat => Assert.True(seat.GetProperty("isFree").GetBoolean()));
    }

    [Fact]
    public async Task Seats_of_a_paid_booking_stay_taken()
    {
        await AddTicketAsync(ConfirmedBookingId, SeatB02);
        Factory.Clock.SetUtcNow(AfterAllSeededHoldsRanOut);

        var seats = await GetSeatsAsync(TripId);

        Assert.False(IsFree(seats, "B02"));
    }

    [Fact]
    public async Task Seats_of_a_cancelled_booking_are_free()
    {
        await AddTicketAsync(CancelledBookingId, SeatB02);

        var seats = await GetSeatsAsync(TripId);

        Assert.True(IsFree(seats, "B02"));
    }

    [Fact]
    public async Task Unknown_train_trip_is_not_found()
    {
        var response = await Client.GetAsync("/api/train-trips/999999/seats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadJsonAsync(response);
        Assert.Equal("TrainTrip.NotFound", problem.GetProperty("code").GetString());
    }

    private async Task<IReadOnlyList<JsonElement>> GetSeatsAsync(int tripId)
    {
        var response = await Client.GetAsync($"/api/train-trips/{tripId}/seats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(tripId, body.GetProperty("trainTripId").GetInt32());
        return body.GetProperty("seats").EnumerateArray().ToList();
    }

    private Task AddTicketAsync(int bookingId, int seatId) => Factory.Database.ExecuteAsync(
        $"INSERT INTO BookingItems (BookingId, TrainTripId, SeatId, PassengerName, PassengerIdCard, Price) " +
        $"VALUES ({bookingId}, {TripId}, {seatId}, 'Test Passenger', '001099000001', 750000.00)");

    private static JsonElement Seat(IEnumerable<JsonElement> seats, string seatNumber) =>
        seats.Single(s => s.GetProperty("seatNumber").GetString() == seatNumber);

    private static bool IsFree(IEnumerable<JsonElement> seats, string seatNumber) =>
        Seat(seats, seatNumber).GetProperty("isFree").GetBoolean();
}
