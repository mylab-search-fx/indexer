using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.DestinationRepository;
using OpenSearch.Client;
using Xunit.Abstractions;

namespace Infrastructure.OpenSearch.IntegrationTests.DestinationRepository;

public class OpenSearchDestinationRepositoryBehavior(ITestOutputHelper output)
{
    [Fact(Timeout = 60000)]
    public async Task ShouldStoreIndexedDocumentFields()
    {
        // Arrange
        var app = TestTools.CreateRepositoryApp(output);
        await using var provider = app;
        var client = provider.GetRequiredService<IOpenSearchClient>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<OpenSearchDestinationRepository>();
        var indexName = "repo" + Guid.NewGuid().ToString("N");
        var repository = new OpenSearchDestinationRepository(indexName, client, logger);
        var page = TestTools.CreatePage("doc-1");
        var item = page.Items[0];

        try
        {
            // Act
            await repository.WriteAsync(page);

            var sourceJson = await TestTools.ReadSingleSourceJsonAsync(client, indexName);
            var stored = TestTools.ReadStored(sourceJson);

            // Assert
            Assert.Equal(item.Id.Value, stored.Id);
            Assert.Equal(item.Seed, stored.Seed);
            Assert.Equal(item.Fields["title"].Value, stored.Title);
            Assert.Equal(item.Fields["active"].Value, stored.Active);
            Assert.Equal(item.Fields["amount"].Value, stored.Amount);
            Assert.Equal("2026-11-23T04:05:06", stored.UpdatedAt);
            Assert.Null(stored.Note);
        }
        finally
        {
            await TestTools.DeleteIndexAsync(client, indexName);
        }
    }
}
