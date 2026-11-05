using System.Net;
using TrainBooking.Api.Tests.Infrastructure;

namespace TrainBooking.Api.Tests.Customers;

public sealed class CurrentCustomerTests : ApiTestBase
{
    // Seed users (init.sql): 1-3 are customers, 4 is staff, 5 is admin.
    private const int SeededCustomerId = 1;
    private const int SeededStaffId = 4;

    public CurrentCustomerTests(TrainBookingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Known_customer_is_identified()
    {
        using var client = ClientFor(SeededCustomerId);

        var response = await client.GetAsync("/api/customers/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await ReadJsonAsync(response);
        Assert.Equal(SeededCustomerId, customer.GetProperty("id").GetInt32());
        Assert.Equal("Nguyen Van A", customer.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Missing_customer_header_is_rejected()
    {
        var response = await Client.GetAsync("/api/customers/me");

        await AssertUnauthorizedAsync(response, "Customer.Missing");
    }

    [Fact]
    public async Task Non_numeric_customer_header_is_rejected()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/customers/me");
        request.Headers.Add("X-Customer-Id", "abc");

        var response = await Client.SendAsync(request);

        await AssertUnauthorizedAsync(response, "Customer.Missing");
    }

    [Fact]
    public async Task Unknown_customer_is_rejected()
    {
        using var client = ClientFor(999_999);

        var response = await client.GetAsync("/api/customers/me");

        await AssertUnauthorizedAsync(response, "Customer.Unknown");
    }

    [Fact]
    public async Task Staff_user_is_not_a_customer()
    {
        using var client = ClientFor(SeededStaffId);

        var response = await client.GetAsync("/api/customers/me");

        await AssertUnauthorizedAsync(response, "Customer.Unknown");
    }

    private static async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedCode)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadJsonAsync(response);
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("detail").GetString()));
    }
}
