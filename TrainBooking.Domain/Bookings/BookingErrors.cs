using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error NoTickets = Error.Validation(
        "Booking.NoTickets",
        "A booking needs at least one ticket.");

    public static readonly Error PassengerNameRequired = Error.Validation(
        "Booking.PassengerNameRequired",
        "Every ticket needs a passenger name.");

    public static readonly Error PassengerIdNumberRequired = Error.Validation(
        "Booking.PassengerIdNumberRequired",
        "Every ticket needs the passenger's identity document number.");

    public static readonly Error SeatTaken = Error.Conflict(
        "Booking.SeatTaken",
        "At least one of the chosen seats is already taken on that train trip.");

    public static readonly Error NotFound = Error.NotFound(
        "Booking.NotFound",
        "No booking with that code exists for this customer.");

    public static Error DuplicateSeat(int trainTripId, int seatId) => Error.Validation(
        "Booking.DuplicateSeat",
        $"Seat {seatId} on train trip {trainTripId} is chosen more than once.");

    public static Error UnknownTrainTrip(int trainTripId) => Error.Validation(
        "Booking.UnknownTrainTrip",
        $"Train trip {trainTripId} does not exist.");

    public static Error UnknownSeat(int seatId) => Error.Validation(
        "Booking.UnknownSeat",
        $"Seat {seatId} does not exist.");

    public static Error TrainTripDeparted(int trainTripId) => Error.Validation(
        "Booking.TrainTripDeparted",
        $"Train trip {trainTripId} has already departed.");

    public static Error SeatNotOnTrainTrip(int trainTripId, int seatId) => Error.Validation(
        "Booking.SeatNotOnTrainTrip",
        $"Seat {seatId} is not on the train running train trip {trainTripId}.");

    public static Error NoFare(int trainTripId, int seatId) => Error.Validation(
        "Booking.NoFare",
        $"Seat {seatId} is not sold on train trip {trainTripId}: its seat class has no fare.");
}
