using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.Options;
using OpenSearch.Client;

namespace MyLab.Search.Indexer.Infrastructure.OpenSearch.DestinationRepository;

class OpenSearchDestinationRepositoryFactory
(
    IOpenSearchClient opensearch, 
    IOptions<IndexingOptions> opts,
    ILogger<OpenSearchDestinationRepository>? logger = null) : IDestinationRepositoryFactory
{
    public IDestinationRepository Create(IndexDescription idxDesc)
    {
        var indexName = BuildIndexName(idxDesc.Name);
        return new OpenSearchDestinationRepository(indexName, opensearch, logger);
    }

    string BuildIndexName(string indexId)
    {
        var builder = new StringBuilder(indexId);

        if (!string.IsNullOrWhiteSpace(opts.Value.IndexPrefix))
            builder.Insert(0, opts.Value.IndexPrefix + "-");
        if (!string.IsNullOrWhiteSpace(opts.Value.IndexSuffix))
            builder.Append(opts.Value.IndexSuffix);

        return builder.ToString();
    }
}