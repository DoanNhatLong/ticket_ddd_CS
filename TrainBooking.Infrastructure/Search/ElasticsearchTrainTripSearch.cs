using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.TrainTrips.Search;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Infrastructure.Entity;

namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// Elasticsearch implementation of <see cref="ITrainTripSearch"/>.
/// MySQL stays the source of truth; the index is a copy that can be rebuilt at any time.
/// </summary>
internal sealed class ElasticsearchTrainTripSearch : ITrainTripSearch
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly SearchIndexBuilder _indexBuilder;
    private readonly AppDbContext _context;

    public ElasticsearchTrainTripSearch(
        ElasticsearchClient client,
        ElasticsearchOptions options,
        SearchIndexBuilder indexBuilder,
        AppDbContext context)
    {
        _client = client;
        _options = options;
        _indexBuilder = indexBuilder;
        _context = context;
    }

    public async Task<Result<TrainTripSearchPage>> SearchAsync(
        TrainTripSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchRequest<TrainTripDocument>(_options.TrainTripIndex)
        {
            Query = BuildQuery(criteria),
            Sort = BuildSort(criteria),
            From = (criteria.Page - 1) * criteria.PageSize,
            Size = criteria.PageSize
        };

        var response = await _client.SearchAsync<TrainTripDocument>(request, cancellationToken);
        if (!response.IsValidResponse)
            return _indexBuilder.Unavailable("searching train trips", response, cancellationToken);

        return new TrainTripSearchPage(
            response.Documents.Select(ToItem).ToList(),
            response.Total,
            criteria.Page,
            criteria.PageSize);
    }

    public async Task<Result<int>> RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var trainTrips = await _context.Traintrips
            .AsNoTracking()
            .Include(t => t.Train)
            .Include(t => t.Fares)
            .Include(t => t.Route).ThenInclude(r => r.DepartureStation)
            .Include(t => t.Route).ThenInclude(r => r.ArrivalStation)
            .ToListAsync(cancellationToken);

        return await _indexBuilder.RebuildAsync(
            _options.TrainTripIndex,
            MapProperties,
            trainTrips.Select(ToDocument).ToList(),
            document => document.TrainTripId,
            cancellationToken);
    }

    private static Query BuildQuery(TrainTripSearchCriteria criteria)
    {
        // Filters only say yes or no; they do not influence the ranking.
        var filters = new List<Query>
        {
            new DateRangeQuery(Infer.Field<TrainTripDocument>(d => d.DepartureTime))
            {
                Gt = criteria.DepartsAfterUtc
            }
        };

        if (criteria.DepartureStationCode is { } from)
            filters.Add(new TermQuery(Infer.Field<TrainTripDocument>(d => d.DepartureStationCode)) { Value = from });

        if (criteria.ArrivalStationCode is { } to)
            filters.Add(new TermQuery(Infer.Field<TrainTripDocument>(d => d.ArrivalStationCode)) { Value = to });

        if (criteria.DepartureDate is { } date)
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            filters.Add(new DateRangeQuery(Infer.Field<TrainTripDocument>(d => d.DepartureTime))
            {
                Gte = dayStart,
                Lt = dayStart.AddDays(1)
            });
        }

        var query = new BoolQuery { Filter = filters };

        if (criteria.Text is { } text)
        {
            var field = Infer.Field<TrainTripDocument>(d => d.SearchText);

            // Every word must match, with small typos allowed after its first character.
            query.Must =
            [
                new MatchQuery(field)
                {
                    Query = text,
                    Operator = Operator.And,
                    Fuzziness = new Fuzziness("AUTO"),
                    PrefixLength = 1
                }
            ];

            // Words typed exactly rank above words that only match with a typo.
            query.Should = [new MatchQuery(field) { Query = text, Boost = 2 }];
        }

        return query;
    }

    private static List<SortOptions> BuildSort(TrainTripSearchCriteria criteria)
    {
        var sort = new List<SortOptions>();

        if (criteria.Text is not null)
            sort.Add(SortOptions.Score(new ScoreSort { Order = SortOrder.Desc }));

        sort.Add(SortOptions.Field(
            Infer.Field<TrainTripDocument>(d => d.DepartureTime),
            new FieldSort { Order = SortOrder.Asc }));

        // Train trips leaving at the same moment keep a stable order across pages.
        sort.Add(SortOptions.Field(
            Infer.Field<TrainTripDocument>(d => d.TrainTripId),
            new FieldSort { Order = SortOrder.Asc }));

        return sort;
    }

    private static void MapProperties(PropertiesDescriptor<TrainTripDocument> properties)
    {
        properties
            .IntegerNumber(d => d.TrainTripId)
            .Keyword(d => d.TrainCode)
            .Keyword(d => d.TrainName)
            .Keyword(d => d.DepartureStationCode)
            .Keyword(d => d.DepartureStationName)
            .Keyword(d => d.DepartureCity)
            .Keyword(d => d.ArrivalStationCode)
            .Keyword(d => d.ArrivalStationName)
            .Keyword(d => d.ArrivalCity)
            .Date(d => d.DepartureTime)
            .Date(d => d.ArrivalTime)
            .Keyword(d => d.Status)
            .DoubleNumber(d => d.LowestFare!)
            .Text(d => d.SearchText, text => text.Analyzer(SearchIndexBuilder.FoldingAnalyzer));
    }

    private static TrainTripDocument ToDocument(Traintrip trip)
    {
        var departure = trip.Route.DepartureStation;
        var arrival = trip.Route.ArrivalStation;

        return new TrainTripDocument
        {
            TrainTripId = trip.Id,
            TrainCode = trip.Train.Code,
            TrainName = trip.Train.Name,
            DepartureStationCode = departure.Code,
            DepartureStationName = departure.Name,
            DepartureCity = departure.City,
            ArrivalStationCode = arrival.Code,
            ArrivalStationName = arrival.Name,
            ArrivalCity = arrival.City,
            DepartureTime = DateTime.SpecifyKind(trip.DepartureTime, DateTimeKind.Utc),
            ArrivalTime = DateTime.SpecifyKind(trip.ArrivalTime, DateTimeKind.Utc),
            Status = trip.Status,
            LowestFare = trip.Fares.Count == 0 ? null : trip.Fares.Min(f => f.Price),
            SearchText = string.Join(' ',
                departure.Name, departure.City, departure.Code,
                arrival.Name, arrival.City, arrival.Code,
                trip.Train.Name, trip.Train.Code)
        };
    }

    private static TrainTripSearchItem ToItem(TrainTripDocument document) => new(
        document.TrainTripId,
        document.TrainCode,
        document.TrainName,
        new TrainTripSearchStation(
            document.DepartureStationCode, document.DepartureStationName, document.DepartureCity),
        new TrainTripSearchStation(
            document.ArrivalStationCode, document.ArrivalStationName, document.ArrivalCity),
        document.DepartureTime,
        document.ArrivalTime,
        document.Status,
        document.LowestFare);
}
