using MediatR;
using TrainBooking.Application.TrainTrips.Search;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Commands.RebuildTrainTripIndex;

public sealed class RebuildTrainTripIndexCommandHandler : IRequestHandler<RebuildTrainTripIndexCommand, Result<int>>
{
    private readonly ITrainTripSearch _trainTripSearch;

    public RebuildTrainTripIndexCommandHandler(ITrainTripSearch trainTripSearch)
    {
        _trainTripSearch = trainTripSearch;
    }

    public Task<Result<int>> Handle(RebuildTrainTripIndexCommand command, CancellationToken cancellationToken)
    {
        return _trainTripSearch.RebuildIndexAsync(cancellationToken);
    }
}
