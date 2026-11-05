using Microsoft.Extensions.DependencyInjection;
using TrainBooking.Api.Tests.Infrastructure;

namespace TrainBooking.Api.Tests;

public sealed class ClockTests : ApiTestBase
{
    public ClockTests(TrainBookingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public void Application_reads_time_from_the_test_clock()
    {
        var clock = Factory.Services.GetRequiredService<TimeProvider>();

        Assert.IsType<TestClock>(clock);
        Assert.Same(Factory.Clock, clock);
    }
}
