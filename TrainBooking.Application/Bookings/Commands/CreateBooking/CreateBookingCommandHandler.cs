using MediatR;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Domain.Bookings;
using TrainBooking.Domain.TrainTrips;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Application.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, Result<BookingDetailResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITrainTripRepository _trainTripRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly TimeProvider _clock;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        ITrainTripRepository trainTripRepository,
        ISeatRepository seatRepository,
        TimeProvider clock)
    {
        _bookingRepository = bookingRepository;
        _trainTripRepository = trainTripRepository;
        _seatRepository = seatRepository;
        _clock = clock;
    }

    public async Task<Result<BookingDetailResponse>> Handle(
        CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var requested = command.Tickets ?? [];

        var trainTrips = new Dictionary<int, TrainTrip>();
        foreach (var trainTripId in requested.Select(t => t.TrainTripId).Distinct())
        {
            var trainTrip = await _trainTripRepository.GetByIdAsync(trainTripId, cancellationToken);
            if (trainTrip is null)
                return BookingErrors.UnknownTrainTrip(trainTripId);

            trainTrips.Add(trainTripId, trainTrip);
        }

        var seatIds = requested.Select(t => t.SeatId).Distinct().ToList();
        var seats = (await _seatRepository.GetByIdsAsync(seatIds, cancellationToken)).ToDictionary(s => s.Id);
        foreach (var seatId in seatIds)
        {
            if (!seats.ContainsKey(seatId))
                return BookingErrors.UnknownSeat(seatId);
        }

        var ticketRequests = requested
            .Select(t => new TicketRequest(
                trainTrips[t.TrainTripId],
                seats[t.SeatId],
                t.PassengerName ?? string.Empty,
                t.PassengerIdNumber ?? string.Empty))
            .ToList();

        var nowUtc = _clock.GetUtcNow().UtcDateTime;

        // CustomerAuthenticationBehavior guarantees a known customer.
        var created = Booking.Create(
            command.CustomerId!.Value,
            BookingCodeGenerator.New(),
            ticketRequests,
            nowUtc);

        if (created.IsFailure)
            return created.Error;

        // Seats whose previous hold has run out go back on sale now, without waiting for the
        // background expiry: the overdue bookings are expired in the same transaction.
        var requestedSeats = created.Value.Tickets.Select(t => (t.TrainTripId, t.SeatId)).ToList();
        var overdue = await _bookingRepository.GetOverdueHoldingSeatsAsync(requestedSeats, nowUtc, cancellationToken);
        foreach (var overdueBooking in overdue)
            overdueBooking.Expire(nowUtc);

        var saved = await _bookingRepository.AddAsync(created.Value, overdue, cancellationToken);
        if (saved.IsFailure)
            return saved.Error;

        // Reload to return the ids the database assigned.
        var booking = await _bookingRepository.GetByCodeAsync(created.Value.BookingCode, cancellationToken);
        return BookingDetailResponse.From(booking!);
    }
}
