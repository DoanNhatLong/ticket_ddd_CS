using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Commands.RebuildTrainTripIndex;

/// <summary>
/// Rebuilds the train trip search data from the database. Returns how many train trips were indexed.
/// </summary>
public sealed record RebuildTrainTripIndexCommand : IRequest<Result<int>>;
