using System.Net.Http.Json;
using System.Text.Json;

namespace TrainBooking.Api.Tests.Infrastructure;

/// <summary>
/// The test database is shared, so API test classes run one at a time.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<TrainBookingApiFactory>
{
    public const string Name = "API";
}

/// <summary>
/// Base class for HTTP-level tests: resets the test database before every test.
/// </summary>
[Collection(ApiCollection.Name)]
public abstract class ApiTestBase : IAsyncLifetime
{
    protected ApiTestBase(TrainBookingApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected TrainBookingApiFactory Factory { get; }

    protected HttpClient Client { get; }

    public Task InitializeAsync()
    {
        Factory.Clock.Reset();
        return Factory.Database.ResetAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a client that identifies itself as the given customer.
    /// </summary>
    protected HttpClient ClientFor(int customerId)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        return client;
    }

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
