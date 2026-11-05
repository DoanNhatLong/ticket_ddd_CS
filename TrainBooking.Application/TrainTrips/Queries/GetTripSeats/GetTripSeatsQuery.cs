using MediatR;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.TrainTrips.Queries.GetTripSeats;

/// <summary>
/// Lists every seat on a train trip with its fare and whether it is free right now.
/// </summary>
public sealed record GetTripSeatsQuery(int TrainTripId) : IRequest<Result<TripSeatsResponse>>;
