using System.Net;
using TrainBooking.Api.Tests.Infrastructure;

namespace TrainBooking.Api.Tests.Bookings;

public sealed class GetAllBookingsTests : ApiTestBase
{
    public GetAllBookingsTests(TrainBookingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Lists_every_seeded_booking()
    {
        var response = await Client.GetAsync("/api/bookings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var bookings = await ReadJsonAsync(response);
        Assert.Equal(5, bookings.GetArrayLength());
        Assert.Contains(
            bookings.EnumerateArray(),
            b => b.GetProperty("bookingCode").GetString() == "BK20261103001");
    }

    [Fact]
    public async Task Unexpected_error_returns_500_with_a_problem_body()
    {
        await Factory.Database.ExecuteAsync("UPDATE Bookings SET Status = 'NotARealStatus' WHERE Id = 1");

        var response = await Client.GetAsync("/api/bookings");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadJsonAsync(response);
        Assert.Equal("Server.Unexpected", problem.GetProperty("code").GetString());
    }
}
