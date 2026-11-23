using System.Text.Json;

namespace MyLab.Search.Indexer.Infrastructure.OpenSearch.Serialization;

static class IndexerJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Options;

    static IndexerJsonSerializerOptions()
    {
        Options = new JsonSerializerOptions();
        Options.Converters.Add(new SourceDataItemJsonConverter());
    }
}