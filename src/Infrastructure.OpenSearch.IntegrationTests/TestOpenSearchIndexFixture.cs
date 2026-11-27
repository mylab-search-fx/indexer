using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Infrastructure.OpenSearch.IntegrationTests;

public class TestOpenSearchIndexFixture<T> : IAsyncLifetime where T : class
{
    IOpenSearchClient? _client;

    public string? IndexName { get; private set; }

    public Task CleanupAsync()
    {
        if (_client == null) throw new InvalidOperationException("The OpenSearch connection is not established");
        return _client.DeleteByQueryAsync<T>(d => d
                .Index(IndexName)
                .Query(q => q.MatchAll())
                .Refresh(Refresh.True)
        );
    }

    public Task InitializeAsync()
    {
        var opensearchApp = OpenSearchTestTools.CreateApp();
        _client = opensearchApp.GetRequiredService<IOpenSearchClient>();

        IndexName = "test-" + Guid.NewGuid().ToString("N");
        return _client.Indices.CreateAsync(IndexName);
    }

    public Task DisposeAsync()
    {
        if (_client != null)
        {
            return _client.Indices.DeleteAsync(IndexName);
        }

        return Task.CompletedTask;
    }
}