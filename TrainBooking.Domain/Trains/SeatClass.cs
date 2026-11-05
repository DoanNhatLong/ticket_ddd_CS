namespace TrainBooking.Domain.Trains;

/// <summary>
/// The kind of place a seat offers (e.g. soft seat, lower berth). The only thing that sets a seat's price.
/// </summary>
public sealed record SeatClass(int Id, string Code, string Name);
