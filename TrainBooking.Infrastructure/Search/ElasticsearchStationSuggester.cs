using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Stations;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Infrastructure.Entity;
using Result = TrainBooking.Domain.Abstractions.Result;

namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// Elasticsearch implementation of <see cref="IStationSuggester"/>, based on a completion suggester.
/// MySQL stays the source of truth; the index is a copy that can be rebuilt at any time.
/// </summary>
internal sealed class ElasticsearchStationSuggester : IStationSuggester
{
    private const string SuggesterName = "stations";
    private const string StationNamePrefix = "Ga ";

    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly SearchIndexBuilder _indexBuilder;
    private readonly AppDbContext _context;

    public ElasticsearchStationSuggester(
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

    public async Task<Result<IReadOnlyList<StationSuggestion>>> SuggestAsync(
        string text,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.SearchAsync<StationDocument>(search => search
            .Index(_options.StationIndex)
            .Suggest(suggest => suggest
                .Suggesters(suggesters => suggesters
                    .Add(SuggesterName, suggester => suggester
                        .Prefix(text)
                        .Completion(completion => completion
                            .Field(document => document.Suggest)
                            .Size(maxCount)
                            // AUTO: no typo up to 2 characters, 1 typo up to 5, 2 typos beyond.
                            .Fuzzy(fuzzy => fuzzy.Fuzziness(new Fuzziness("AUTO"))))))),
            cancellationToken);

        if (!response.IsValidResponse)
            return _indexBuilder.Unavailable("suggesting stations", response, cancellationToken);

        var suggestions = (response.Suggest?.GetCompletion(SuggesterName) ?? [])
            .SelectMany(entry => entry.Options)
            .Select(option => option.Source)
            .OfType<StationDocument>()
            .DistinctBy(document => document.StationId)
            .Select(document => new StationSuggestion(
                document.StationId,
                document.Code,
                document.Name,
                document.City))
            .ToList();

        return Result.Success<IReadOnlyList<StationSuggestion>>(suggestions);
    }

    public async Task<Result<int>> RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var stations = await _context.Stations.AsNoTracking().ToListAsync(cancellationToken);

        return await _indexBuilder.RebuildAsync(
            _options.StationIndex,
            MapProperties,
            stations.Select(ToDocument).ToList(),
            document => document.StationId,
            cancellationToken);
    }

    private static void MapProperties(PropertiesDescriptor<StationDocument> properties)
    {
        properties
            .IntegerNumber(document => document.StationId)
            .Keyword(document => document.Code)
            .Keyword(document => document.Name)
            .Keyword(document => document.City)
            .Completion(document => document.Suggest, completion => completion
                .Analyzer(SearchIndexBuilder.FoldingAnalyzer));
    }

    private static StationDocument ToDocument(Station station)
    {
        // A completion field only matches from the start of a text, so "Ga Hà Nội" alone
        // would not be found by typing "ha".
        var nameWithoutPrefix = station.Name.StartsWith(StationNamePrefix, StringComparison.OrdinalIgnoreCase)
            ? station.Name[StationNamePrefix.Length..]
            : station.Name;

        return new StationDocument
        {
            StationId = station.Id,
            Code = station.Code,
            Name = station.Name,
            City = station.City,
            Suggest = new[] { station.Name, nameWithoutPrefix, station.City, station.Code }.Distinct().ToList()
        };
    }
}
