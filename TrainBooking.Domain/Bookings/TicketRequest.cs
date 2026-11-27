using TrainBooking.Domain.TrainTrips;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Domain.Bookings;

/// <summary>
/// One seat a customer wants in a new booking, with the passenger who will use it.
/// <see cref="ExpectedPrice"/> is the price the customer was shown; it is only compared with the
/// current fare and never used as the ticket price.
/// </summary>
public sealed record TicketRequest(
    TrainTrip TrainTrip,
    Seat Seat,
    string PassengerName,
    string PassengerIdNumber,
    decimal? ExpectedPrice);
