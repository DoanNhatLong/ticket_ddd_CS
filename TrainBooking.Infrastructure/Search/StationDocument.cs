namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// One station as stored in Elasticsearch.
/// </summary>
internal sealed class StationDocument
{
    public int StationId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Every text a customer may start typing to find this station (completion field).
    /// </summary>
    public IReadOnlyCollection<string> Suggest { get; set; } = [];
}
