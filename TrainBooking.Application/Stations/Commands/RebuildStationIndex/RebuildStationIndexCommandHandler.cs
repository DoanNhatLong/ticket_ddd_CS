using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Stations.Commands.RebuildStationIndex;

public sealed class RebuildStationIndexCommandHandler : IRequestHandler<RebuildStationIndexCommand, Result<int>>
{
    private readonly IStationSuggester _stationSuggester;

    public RebuildStationIndexCommandHandler(IStationSuggester stationSuggester)
    {
        _stationSuggester = stationSuggester;
    }

    public Task<Result<int>> Handle(RebuildStationIndexCommand command, CancellationToken cancellationToken)
    {
        return _stationSuggester.RebuildIndexAsync(cancellationToken);
    }
}
