namespace TrainBooking.Domain.Bookings;

public enum BookingStatus
{
    /// <summary>Created and not paid yet; its tickets hold their seats until the seat hold runs out.</summary>
    Pending,

    Confirmed,

    Completed,

    /// <summary>Every ticket was cancelled by the customer.</summary>
    Cancelled,

    /// <summary>The seat hold ran out before payment; the seats are back on sale.</summary>
    Expired
}
