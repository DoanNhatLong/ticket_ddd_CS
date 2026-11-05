using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TrainBooking.Api.Tests.Infrastructure;

/// <summary>
/// Runs the real API in memory against the MySQL test database, with a controllable clock.
/// </summary>
public sealed class TrainBookingApiFactory : WebApplicationFactory<Program>
{
    public TrainBookingApiFactory()
    {
        Database = new TestDatabase();
    }

    public TestDatabase Database { get; }

    /// <summary>
    /// The application's clock. Tests move time with <see cref="TestClock.Advance"/> or
    /// <see cref="TestClock.SetUtcNow"/> instead of sleeping; it is reset before every test.
    /// </summary>
    public TestClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" keeps Development-only sources (user secrets with the dev connection string) out.
        builder.UseEnvironment("Testing");

        // Program.cs reads the connection string before the host is built, which is too early for
        // ConfigureAppConfiguration callbacks; environment variables are already part of builder.Configuration.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", Database.ConnectionString);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
