using MediatR;
using TrainBooking.Application.Stations.Commands.RebuildStationIndex;

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

            var result = await sender.Send(new RebuildStationIndexCommand(), stoppingToken);

            if (result.IsSuccess)
                _logger.LogInformation("Indexed {Count} stations.", result.Value);
            else
                _logger.LogWarning("Station index was not built: {Code}.", result.Error.Code);
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
}
