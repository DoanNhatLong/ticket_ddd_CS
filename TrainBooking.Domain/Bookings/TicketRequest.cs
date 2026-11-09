using TrainBooking.Domain.TrainTrips;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Domain.Bookings;

/// <summary>
/// One seat a customer wants in a new booking, with the passenger who will use it.
/// </summary>
public sealed record TicketRequest(
    TrainTrip TrainTrip,
    Seat Seat,
    string PassengerName,
    string PassengerIdNumber);
