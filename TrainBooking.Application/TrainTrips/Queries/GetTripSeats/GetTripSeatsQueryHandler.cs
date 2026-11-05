using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;
using TrainBooking.Domain.TrainTrips;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Application.TrainTrips.Queries.GetTripSeats;

public sealed class GetTripSeatsQueryHandler
    : IRequestHandler<GetTripSeatsQuery, Result<TripSeatsResponse>>
{
    private readonly ITrainTripRepository _trainTripRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _clock;

    public GetTripSeatsQueryHandler(
        ITrainTripRepository trainTripRepository,
        ISeatRepository seatRepository,
        IBookingRepository bookingRepository,
        TimeProvider clock)
    {
        _trainTripRepository = trainTripRepository;
        _seatRepository = seatRepository;
        _bookingRepository = bookingRepository;
        _clock = clock;
    }

    public async Task<Result<TripSeatsResponse>> Handle(
        GetTripSeatsQuery request,
        CancellationToken cancellationToken)
    {
        var trainTrip = await _trainTripRepository.GetByIdAsync(request.TrainTripId, cancellationToken);
        if (trainTrip is null)
            return TrainTripErrors.NotFound(request.TrainTripId);

        var seats = await _seatRepository.GetByTrainAsync(trainTrip.TrainId, cancellationToken);
        var takenSeatIds = await _bookingRepository.GetTakenSeatIdsAsync(
            trainTrip.Id,
            _clock.GetUtcNow().UtcDateTime,
            cancellationToken);

        var seatResponses = seats
            .Select(seat => new TripSeatResponse(
                seat.Id,
                seat.CarriageNumber,
                seat.SeatNumber,
                seat.SeatClass.Code,
                seat.SeatClass.Name,
                trainTrip.FareFor(seat.SeatClass.Id)?.Price,
                IsFree: !takenSeatIds.Contains(seat.Id)))
            .ToList();

        return new TripSeatsResponse(trainTrip.Id, trainTrip.DepartureTime, trainTrip.ArrivalTime, seatResponses);
    }
}
