using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.Extensions.Logging;
using TrainBooking.Application.Abstractions;
using TrainBooking.Domain.Abstractions;
using Error = TrainBooking.Domain.Abstractions.Error;

namespace TrainBooking.Infrastructure.Search;

/// <summary>
/// Shared by the search adapters: rebuilds an index without interrupting searches, and turns
/// Elasticsearch failures into <see cref="SearchErrors.Unavailable"/>.
/// </summary>
internal sealed class SearchIndexBuilder
{
    /// <summary>
    /// Analyzer ignoring case and accents: "ha noi" matches "Hà Nội".
    /// </summary>
    public const string FoldingAnalyzer = "folding";

    private readonly ElasticsearchClient _client;
    private readonly ILogger<SearchIndexBuilder> _logger;

    public SearchIndexBuilder(ElasticsearchClient client, ILogger<SearchIndexBuilder> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Replaces everything behind <paramref name="alias"/> with <paramref name="documents"/>.
    /// </summary>
    public async Task<Result<int>> RebuildAsync<TDocument>(
        string alias,
        Action<PropertiesDescriptor<TDocument>> mapProperties,
        IReadOnlyCollection<TDocument> documents,
        Func<TDocument, int> getId,
        CancellationToken cancellationToken)
        where TDocument : class
    {
        // Fill a new index first and only then point the alias to it, so searches never see
        // a missing or half-filled index.
        var newIndex = $"{alias}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var created = await CreateIndexAsync(newIndex, mapProperties, cancellationToken);
        if (!created.IsValidResponse)
            return Unavailable($"creating index {newIndex}", created, cancellationToken);

        if (documents.Count > 0)
        {
            var indexed = await _client.BulkAsync(bulk => bulk
                .Index(newIndex)
                .Refresh(Refresh.True)
                .IndexMany(documents, (operation, document) => operation.Id(getId(document))),
                cancellationToken);

            if (!indexed.IsValidResponse || indexed.Errors)
            {
                await _client.Indices.DeleteAsync(newIndex, CancellationToken.None);
                return Unavailable($"filling index {newIndex}", indexed, cancellationToken);
            }
        }

        var oldIndices = await GetIndicesBehindAliasAsync(alias, cancellationToken);

        var actions = oldIndices
            .Select(index => IndexUpdateAliasesAction.Remove(new RemoveAction { Index = index, Alias = alias }))
            .Append(IndexUpdateAliasesAction.Add(new AddAction { Index = newIndex, Alias = alias }))
            .ToList();

        var switched = await _client.Indices.UpdateAliasesAsync(
            new UpdateAliasesRequest { Actions = actions },
            cancellationToken);

        if (!switched.IsValidResponse)
        {
            await _client.Indices.DeleteAsync(newIndex, CancellationToken.None);
            return Unavailable($"switching alias {alias}", switched, cancellationToken);
        }

        foreach (var oldIndex in oldIndices)
            await _client.Indices.DeleteAsync(oldIndex, cancellationToken);

        return documents.Count;
    }

    public Error Unavailable(string action, ElasticsearchResponse response, CancellationToken cancellationToken)
    {
        // A request the caller gave up on is not an Elasticsearch failure.
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogWarning("Elasticsearch failed while {Action}: {Details}", action, response.DebugInformation);
        return SearchErrors.Unavailable;
    }

    private Task<CreateIndexResponse> CreateIndexAsync<TDocument>(
        string index,
        Action<PropertiesDescriptor<TDocument>> mapProperties,
        CancellationToken cancellationToken)
    {
        return _client.Indices.CreateAsync<TDocument>(index, create => create
            .Settings(settings => settings
                // One node: a replica could never be placed.
                .NumberOfReplicas(0)
                .Analysis(analysis => analysis
                    .Analyzers(analyzers => analyzers
                        .Custom(FoldingAnalyzer, custom => custom
                            .Tokenizer("standard")
                            .Filter(["lowercase", "asciifolding"])))))
            .Mappings(mappings => mappings.Properties(mapProperties)),
            cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetIndicesBehindAliasAsync(
        string alias,
        CancellationToken cancellationToken)
    {
        var response = await _client.Indices.GetAliasAsync(request => request.Name(alias), cancellationToken);

        // 404 (the first build): the alias does not exist yet.
        return response.IsValidResponse ? response.Aliases.Keys.Select(index => index.ToString()).ToList() : [];
    }
}
