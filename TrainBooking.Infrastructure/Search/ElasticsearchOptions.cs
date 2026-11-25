namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// Settings from the <c>Elasticsearch</c> configuration section.
/// </summary>
/// <param name="Url">Address of the Elasticsearch server.</param>
/// <param name="TrainTripIndex">Name (alias) under which train trips are searched.</param>
internal sealed record ElasticsearchOptions(Uri Url, string TrainTripIndex);
