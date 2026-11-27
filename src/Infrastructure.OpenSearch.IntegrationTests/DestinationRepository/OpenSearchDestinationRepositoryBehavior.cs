using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.DestinationRepository;
using OpenSearch.Client;
using Xunit.Abstractions;
using static Infrastructure.OpenSearch.IntegrationTests.OpenSearchTestTools;

namespace Infrastructure.OpenSearch.IntegrationTests.DestinationRepository;

public class OpenSearchDestinationRepositoryBehavior(TestOpenSearchIndexFixture<StoredDocument> fxt, ITestOutputHelper output) 
    : IClassFixture<TestOpenSearchIndexFixture<StoredDocument>>
{
    [Fact(Timeout = 60000)]
    public async Task ShouldStoreIndexedDocumentFields()
    {
        // Arrange
        if (fxt.IndexName == null) throw new InvalidOperationException("Index is not ready");

        var app = CreateApp(output);

        var client = app.GetRequiredService<IOpenSearchClient>();
        var logger = app.GetRequiredService<ILoggerFactory>()
            .CreateLogger<OpenSearchDestinationRepository>();
        var repository = new OpenSearchDestinationRepository(fxt.IndexName, client, logger);
        
        var page = CreatePage("doc-1");
        var item = page.Items[0];

        try
        {
            // Act
            await repository.WriteAsync(page);

            var sourceJson = await ReadSingleSourceJsonAsync(client, fxt.IndexName);
            var stored = ReadStored(sourceJson);

            // Assert
            Assert.Equal(item.Id.Value, stored.Id);
            Assert.Equal(item.Fields["title"].Value, stored.Title);
            Assert.Equal(item.Fields["active"].Value, stored.Active);
            Assert.Equal(item.Fields["amount"].Value, stored.Amount);
            Assert.Equal("2026-11-23T04:05:06", stored.UpdatedAt);
            Assert.Null(stored.Note);
        }
        finally
        {
            await DeleteIndexAsync(client, fxt.IndexName);
        }
    }
}
