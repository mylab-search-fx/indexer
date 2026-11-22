using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log;
using MyLab.Log.XUnit;
using MyLab.OpenSearch.Options;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.Serialization;
using OpenSearch.Client;
using OpenSearch.Net;
using Xunit.Abstractions;

namespace Infrastructure.OpenSearch.IntegrationTests;

static class TestTools
{
    // OpenSearch from test/docker-compose.yml, published port 19200.
    public static OpenSearchOptions LocalOpenSearch => new()
    {
        Host = "https://localhost:19200",
        BasicAuth = new OpenSearchBasicAuthentication
        {
            User = "admin",
            Password = ",w^&i!.m6A"
        },
        IgnoreServerSslCertificate = true
    };

    public static void ConfigureLocalOpenSearch(OpenSearchOptions options)
    {
        var source = LocalOpenSearch;
        options.Host = source.Host;
        options.BasicAuth = source.BasicAuth;
        options.IgnoreServerSslCertificate = source.IgnoreServerSslCertificate;
    }

    public static ServiceProvider CreateRepositoryApp(ITestOutputHelper output)
    {
        var services = new ServiceCollection();

        services
            .AddLogging(logging => logging
                .AddMyLabConsole()
                .AddXUnit(output))
            .AddOpenSearch()
            .ConfigureOpenSearch(ConfigureLocalOpenSearch)
            .ConfigureOpenSearch(options => options.JsonSerializerOptions = IndexerJsonSerializerOptions.Options);

        return services.BuildServiceProvider();
    }

    public static SourceData CreatePage(string id)
    {
        var updatedAt = new DateTime(2026, 11, 23, 4, 5, 6);
        var item = new SourceDataItem
        {
            Id = SourceDataValue.ForValue(id),
            Seed = 42,
            Fields = new Dictionary<string, SourceDataValue>
            {
                ["title"] = SourceDataValue.ForValue("alpha"),
                ["active"] = SourceDataValue.ForValue(true),
                ["amount"] = SourceDataValue.ForValue(12.5d),
                ["updated_at"] = SourceDataValue.ForValue(updatedAt),
                ["note"] = SourceDataValue.Null
            }
        };

        return new SourceData
        {
            Count = 1,
            Full = false,
            NewSeed = 42,
            Items = [item]
        };
    }

    public static async Task<string> ReadSingleSourceJsonAsync(IOpenSearchClient client, string indexName)
    {
        var refresh = await client.LowLevel.Indices.RefreshAsync<StringResponse>(indexName);
        if (!refresh.Success)
            throw new InvalidOperationException(refresh.Body);

        var query = PostData.String("""{"size":10,"query":{"match_all":{}}}""");
        var search = await client.LowLevel.SearchAsync<StringResponse>(indexName, query);
        if (!search.Success)
            throw new InvalidOperationException(search.Body);

        var body = search.Body ?? "";
        using var document = JsonDocument.Parse(body);
        var hits = document.RootElement.GetProperty("hits").GetProperty("hits");
        if (hits.GetArrayLength() != 1)
            throw new InvalidOperationException($"Expected one indexed document in '{indexName}'. Response: {body}");

        var source = hits[0].GetProperty("_source");
        return source.GetRawText();
    }

    public static StoredDocument ReadStored(string sourceJson)
    {
        using var document = JsonDocument.Parse(sourceJson);
        var source = document.RootElement;
        var fields = source.GetProperty("Fields");
        var note = fields.GetProperty("note").GetProperty("Value");

        return new StoredDocument(
            source.GetProperty("Id").GetProperty("Value").GetString(),
            source.GetProperty("Seed").GetInt64(),
            fields.GetProperty("title").GetProperty("Value").GetString(),
            fields.GetProperty("active").GetProperty("Value").GetBoolean(),
            fields.GetProperty("amount").GetProperty("Value").GetDouble(),
            fields.GetProperty("updated_at").GetProperty("Value").GetString(),
            note.ValueKind == JsonValueKind.Null ? null : note.GetString());
    }

    public sealed record StoredDocument(
        string? Id,
        long Seed,
        string? Title,
        bool Active,
        double Amount,
        string? UpdatedAt,
        string? Note);

    public static async Task DeleteIndexAsync(IOpenSearchClient client, string indexName)
    {
        var deleted = await client.LowLevel.Indices.DeleteAsync<StringResponse>(indexName);
        _ = deleted;
    }
}
