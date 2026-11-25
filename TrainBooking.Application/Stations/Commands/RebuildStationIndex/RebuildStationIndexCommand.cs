using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Stations.Commands.RebuildStationIndex;

/// <summary>
/// Rebuilds the station suggestions from the stations in the database. Returns how many were indexed.
/// </summary>
public sealed record RebuildStationIndexCommand : IRequest<Result<int>>;
