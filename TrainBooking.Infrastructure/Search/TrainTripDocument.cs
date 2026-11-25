namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// One train trip as stored in Elasticsearch. Free seats are not here: they change all the time
/// and always come from MySQL.
/// </summary>
internal sealed class TrainTripDocument
{
    public int TrainTripId { get; set; }

    public string TrainCode { get; set; } = string.Empty;

    public string TrainName { get; set; } = string.Empty;

    public string DepartureStationCode { get; set; } = string.Empty;

    public string DepartureStationName { get; set; } = string.Empty;

    public string DepartureCity { get; set; } = string.Empty;

    public string ArrivalStationCode { get; set; } = string.Empty;

    public string ArrivalStationName { get; set; } = string.Empty;

    public string ArrivalCity { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal? LowestFare { get; set; }

    /// <summary>
    /// Every word a free-text search may match: stations, cities and train together, so that
    /// "sai gon da nang" can match both ends of the train trip.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
}
