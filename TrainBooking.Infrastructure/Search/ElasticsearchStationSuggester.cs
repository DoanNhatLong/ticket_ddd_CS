using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainBooking.Application.Abstractions;
using TrainBooking.Application.Stations;
using TrainBooking.Domain.Abstractions;
using TrainBooking.Infrastructure.Entity;
using Error = TrainBooking.Domain.Abstractions.Error;
using Result = TrainBooking.Domain.Abstractions.Result;

namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// Elasticsearch implementation of <see cref="IStationSuggester"/>, based on a completion suggester.
/// MySQL stays the source of truth; the index is a copy that can be rebuilt at any time.
/// </summary>
internal sealed class ElasticsearchStationSuggester : IStationSuggester
{
    private const string FoldingAnalyzer = "folding";
    private const string SuggesterName = "stations";
    private const string StationNamePrefix = "Ga ";

    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly AppDbContext _context;
    private readonly ILogger<ElasticsearchStationSuggester> _logger;

    public ElasticsearchStationSuggester(
        ElasticsearchClient client,
        ElasticsearchOptions options,
        AppDbContext context,
        ILogger<ElasticsearchStationSuggester> logger)
    {
        _client = client;
        _options = options;
        _context = context;
        _logger = logger;
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
            return Unavailable("suggesting stations", response, cancellationToken);

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
        var documents = stations.Select(ToDocument).ToList();

        // Fill a new index first and only then point the alias to it, so suggestions never see
        // a missing or half-filled index.
        var newIndex = $"{_options.StationIndex}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var created = await CreateIndexAsync(newIndex, cancellationToken);
        if (!created.IsValidResponse)
            return Unavailable("creating the station index", created, cancellationToken);

        if (documents.Count > 0)
        {
            var indexed = await _client.BulkAsync(bulk => bulk
                .Index(newIndex)
                .Refresh(Refresh.True)
                .IndexMany(documents, (operation, document) => operation.Id(document.StationId)),
                cancellationToken);

            if (!indexed.IsValidResponse || indexed.Errors)
            {
                await _client.Indices.DeleteAsync(newIndex, CancellationToken.None);
                return Unavailable("indexing stations", indexed, cancellationToken);
            }
        }

        var oldIndices = await GetIndicesBehindAliasAsync(cancellationToken);

        var actions = oldIndices
            .Select(index => IndexUpdateAliasesAction.Remove(new RemoveAction
            {
                Index = index,
                Alias = _options.StationIndex
            }))
            .Append(IndexUpdateAliasesAction.Add(new AddAction
            {
                Index = newIndex,
                Alias = _options.StationIndex
            }))
            .ToList();

        var switched = await _client.Indices.UpdateAliasesAsync(
            new UpdateAliasesRequest { Actions = actions },
            cancellationToken);

        if (!switched.IsValidResponse)
        {
            await _client.Indices.DeleteAsync(newIndex, CancellationToken.None);
            return Unavailable("switching the station index", switched, cancellationToken);
        }

        foreach (var oldIndex in oldIndices)
            await _client.Indices.DeleteAsync(oldIndex, cancellationToken);

        return documents.Count;
    }

    private Task<CreateIndexResponse> CreateIndexAsync(string index, CancellationToken cancellationToken)
    {
        return _client.Indices.CreateAsync<StationDocument>(index, create => create
            .Settings(settings => settings
                // One node: a replica could never be placed.
                .NumberOfReplicas(0)
                .Analysis(analysis => analysis
                    .Analyzers(analyzers => analyzers
                        // Ignores case and accents: "ha noi" matches "Hà Nội".
                        .Custom(FoldingAnalyzer, custom => custom
                            .Tokenizer("standard")
                            .Filter(["lowercase", "asciifolding"])))))
            .Mappings(mappings => mappings
                .Properties(properties => properties
                    .IntegerNumber(document => document.StationId)
                    .Keyword(document => document.Code)
                    .Keyword(document => document.Name)
                    .Keyword(document => document.City)
                    .Completion(document => document.Suggest, completion => completion
                        .Analyzer(FoldingAnalyzer)))),
            cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetIndicesBehindAliasAsync(CancellationToken cancellationToken)
    {
        var response = await _client.Indices.GetAliasAsync(
            alias => alias.Name(_options.StationIndex),
            cancellationToken);

        // 404 (the first build): the alias does not exist yet.
        return response.IsValidResponse ? response.Aliases.Keys.Select(index => index.ToString()).ToList() : [];
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

    private Error Unavailable(string action, ElasticsearchResponse response, CancellationToken cancellationToken)
    {
        // A request the caller gave up on is not an Elasticsearch failure.
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogWarning("Elasticsearch failed while {Action}: {Details}", action, response.DebugInformation);
        return SearchErrors.Unavailable;
    }
}
