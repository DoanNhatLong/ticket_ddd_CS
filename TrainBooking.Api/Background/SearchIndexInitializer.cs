using MediatR;
using TrainBooking.Application.Stations.Commands.RebuildStationIndex;
using TrainBooking.Application.TrainTrips.Commands.RebuildTrainTripIndex;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Api.Background;

/// <summary>
/// Fills the search indices once when the API starts. The API keeps running when that fails;
/// search answers "unavailable" until the index is rebuilt.
/// Setting: <c>Elasticsearch:RebuildOnStartup</c> (default true).
/// </summary>
public sealed class SearchIndexInitializer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchIndexInitializer> _logger;
    private readonly bool _enabled;

    public SearchIndexInitializer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SearchIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _enabled = configuration.GetValue("Elasticsearch:RebuildOnStartup", true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Rebuilding the search indices on startup is disabled.");
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            Report("stations", await sender.Send(new RebuildStationIndexCommand(), stoppingToken));
            Report("train trips", await sender.Send(new RebuildTrainTripIndexCommand(), stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception exception)
        {
            // For example the database is down; the API must still start.
            _logger.LogError(exception, "Building the search indices failed.");
        }
    }

    private void Report(string what, Result<int> result)
    {
        if (result.IsSuccess)
            _logger.LogInformation("Indexed {Count} {What:l}.", result.Value, what);
        else
            _logger.LogWarning("The index of {What:l} was not built: {Code}.", what, result.Error.Code);
    }
}
