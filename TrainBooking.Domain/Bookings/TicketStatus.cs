namespace TrainBooking.Domain.Bookings;

public enum TicketStatus
{
    /// <summary>The ticket's booking is not paid yet; the seat is under a seat hold.</summary>
    Held,

    /// <summary>Paid for, not cancelled, and its train trip has not finished.</summary>
    Active,

    Cancelled,

    /// <summary>Not cancelled, and its train trip has finished.</summary>
    Used
}
