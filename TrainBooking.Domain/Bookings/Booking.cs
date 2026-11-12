using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Bookings;

/// <summary>
/// Booking aggregate root: one purchase made by a customer in a single checkout,
/// grouping one or more tickets (possibly on different train trips) under one booking code.
/// </summary>
public sealed class Booking : Entity
{
    /// <summary>
    /// How long an unpaid booking holds its seats.
    /// </summary>
    public static readonly TimeSpan SeatHoldDuration = TimeSpan.FromMinutes(5);

    private readonly List<Ticket> _tickets;

    private Booking(
        int id,
        int userId,
        string bookingCode,
        decimal totalAmount,
        BookingStatus status,
        DateTime? createdAt,
        DateTime expiresAt,
        IEnumerable<Ticket> tickets)
        : base(id)
    {
        UserId = userId;
        BookingCode = bookingCode;
        TotalAmount = totalAmount;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        _tickets = tickets.ToList();
    }

    /// <summary>
    /// The customer who placed and owns the booking.
    /// </summary>
    public int UserId { get; private set; }

    public string BookingCode { get; private set; }

    /// <summary>
    /// Booking total: the amount charged at checkout. Never changes afterwards.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    /// <summary>
    /// When the seat hold of an unpaid booking runs out (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    public IReadOnlyList<Ticket> Tickets => _tickets;

    /// <summary>
    /// Creates a pending booking that holds the requested seats for <see cref="SeatHoldDuration"/>.
    /// Each ticket price is fixed at the train trip's current fare for the seat's seat class.
    /// </summary>
    public static Result<Booking> Create(
        int customerId,
        string bookingCode,
        IReadOnlyList<TicketRequest> requests,
        DateTime nowUtc)
    {
        if (requests.Count == 0)
            return BookingErrors.NoTickets;

        var tickets = new List<Ticket>(requests.Count);
        var chosenSeats = new HashSet<(int TrainTripId, int SeatId)>();

        foreach (var request in requests)
        {
            var trainTrip = request.TrainTrip;
            var seat = request.Seat;

            if (string.IsNullOrWhiteSpace(request.PassengerName))
                return BookingErrors.PassengerNameRequired;

            if (string.IsNullOrWhiteSpace(request.PassengerIdNumber))
                return BookingErrors.PassengerIdNumberRequired;

            if (trainTrip.DepartureTime <= nowUtc)
                return BookingErrors.TrainTripDeparted(trainTrip.Id);

            if (seat.TrainId != trainTrip.TrainId)
                return BookingErrors.SeatNotOnTrainTrip(trainTrip.Id, seat.Id);

            var fare = trainTrip.FareFor(seat.SeatClass.Id);
            if (fare is null)
                return BookingErrors.NoFare(trainTrip.Id, seat.Id);

            if (!chosenSeats.Add((trainTrip.Id, seat.Id)))
                return BookingErrors.DuplicateSeat(trainTrip.Id, seat.Id);

            tickets.Add(Ticket.CreateHeld(
                trainTrip.Id,
                seat.Id,
                request.PassengerName.Trim(),
                request.PassengerIdNumber.Trim(),
                fare.Price));
        }

        return new Booking(
            id: 0,
            customerId,
            bookingCode,
            totalAmount: tickets.Sum(t => t.Price),
            BookingStatus.Pending,
            createdAt: nowUtc,
            expiresAt: nowUtc + SeatHoldDuration,
            tickets);
    }

    /// <summary>
    /// True when the booking is still unpaid and its seat hold has run out at <paramref name="nowUtc"/>.
    /// Such a booking must be expired before its seats can be sold again.
    /// </summary>
    public bool IsSeatHoldOverdue(DateTime nowUtc) =>
        Status == BookingStatus.Pending && ExpiresAt <= nowUtc;

    /// <summary>
    /// Marks an unpaid booking whose seat hold has run out as expired and puts its seats back on sale.
    /// </summary>
    public Result Expire(DateTime nowUtc)
    {
        if (Status != BookingStatus.Pending)
            return BookingErrors.NotPending;

        if (!IsSeatHoldOverdue(nowUtc))
            return BookingErrors.SeatHoldNotOver;

        Status = BookingStatus.Expired;
        ReleaseHeldTickets();
        return Result.Success();
    }

    /// <summary>
    /// Cancels an unpaid booking at the customer's request before its seat hold runs out.
    /// Its seats go back on sale; nothing was paid, so nothing is refunded.
    /// </summary>
    public Result CancelUnpaid(DateTime nowUtc)
    {
        if (IsSeatHoldOverdue(nowUtc) || Status == BookingStatus.Expired)
            return BookingErrors.Expired;

        if (Status != BookingStatus.Pending)
            return BookingErrors.NotPending;

        Status = BookingStatus.Cancelled;
        ReleaseHeldTickets();
        return Result.Success();
    }

    private void ReleaseHeldTickets()
    {
        foreach (var ticket in _tickets)
            ticket.ReleaseHold();
    }

    /// <summary>
    /// Rebuilds an existing booking from persisted state. Intended for repositories only.
    /// </summary>
    public static Booking Rehydrate(
        int id,
        int userId,
        string bookingCode,
        decimal totalAmount,
        BookingStatus status,
        DateTime? createdAt,
        DateTime expiresAt,
        IEnumerable<Ticket>? tickets = null)
    {
        return new Booking(id, userId, bookingCode, totalAmount, status, createdAt, expiresAt, tickets ?? []);
    }
}
