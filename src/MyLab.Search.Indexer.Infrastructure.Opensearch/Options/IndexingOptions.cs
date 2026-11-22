using MyLab.OpenSearch.Options;

namespace MyLab.Search.Indexer.Infrastructure.OpenSearch.Options;

public class IndexingOptions
{
    public const string SectionName = OpenSearchOptions.SectionName + ":Indexing";

    public string? IndexPrefix;
    public string? IndexSuffix;
}