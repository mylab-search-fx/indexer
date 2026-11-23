using Microsoft.Extensions.Logging;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Data;
using OpenSearch.Client;

namespace MyLab.Search.Indexer.Infrastructure.OpenSearch.DestinationRepository;

class OpenSearchDestinationRepository
(
    string indexName, 
    IOpenSearchClient opensearch,
    ILogger? logger = null
) : IDestinationRepository
{
    readonly IDslLogger? _log = logger?.Dsl();
    
    public async Task WriteAsync(SourceData data)
    {
        if (data.Items.Length == 0)
            return;

        _log?.Debug("Start index data")
            .AndFactIs("index", indexName)
            .AndFactIs("count", data.Count)
            .Write();

        var indexResult = await opensearch.IndexManyAsync(data.Items, indexName);

        if (indexResult.IsValid)
        {
            _log?.Action("Data has been indexed")
                .AndFactIs("index", indexName)
                .AndFactIs("count", indexResult.Took)
                .Write();
        }
        else
        {
            throw new InvalidOperationException("Indexing request error")
                .AndFactIs("exception", indexResult.OriginalException)
                .AndFactIs("server-error", indexResult.ServerError)
                .AndFactIs("debug", indexResult.DebugInformation);
        }
    }
}