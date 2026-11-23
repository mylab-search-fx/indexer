using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log;
using MyLab.Log.XUnit;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using OpenSearch.Client;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Infrastructure.OpenSearch.IntegrationTests;

public class ServiceCollectionExtensionsBehavior(ITestOutputHelper output)
{
    [Fact(Timeout = 60000)]
    public async Task ShouldStoreIndexedFieldsUnderConfiguredIndexName()
    {
        // Arrange
        const string indexPrefix = "it";
        const string indexSuffix = "-demo";
        var services = new ServiceCollection();
        services
            .AddLogging(logging => logging
                .AddMyLabConsole()
                .AddXUnit(output)
                .AddFilter(l => l >= LogLevel.Debug))
            .AddOpenSearchInfrastructure()
            .ConfigureOpenSearchInfrastructure(
                indexing =>
                {
                    indexing.IndexPrefix = indexPrefix;
                    indexing.IndexSuffix = indexSuffix;
                },
                TestTools.ConfigureLocalOpenSearch);
        var app = services.BuildServiceProvider();
        await using var provider = app;
        var client = provider.GetRequiredService<IOpenSearchClient>();
        var factory = provider.GetRequiredService<IDestinationRepositoryFactory>();
        var indexId = "orders" + Guid.NewGuid().ToString("N");
        var indexName = indexPrefix + "-" + indexId + indexSuffix;
        var repository = factory.Create(new IndexDescription(indexId, "select 1", new IndexConfig()));
        var page = TestTools.CreatePage("doc-2");
        var item = page.Items[0];
        TestTools.StoredDocument stored;

        try
        {
            // Act
            await repository.WriteAsync(page);

            var sourceJson = await TestTools.ReadSingleSourceJsonAsync(client, indexName);
            stored = TestTools.ReadStored(sourceJson);

            
        }
        finally
        {
            await TestTools.DeleteIndexAsync(client, indexName);
        }

        // Assert
        Assert.Equal(item.Id.Value, stored.Id);
        Assert.Equal(item.Fields["title"].Value, stored.Title);
        Assert.Equal(item.Fields["active"].Value, stored.Active);
        Assert.Equal(item.Fields["amount"].Value, stored.Amount);
        Assert.Equal("2026-11-23T04:05:06", stored.UpdatedAt);
        Assert.Null(stored.Note);
    }
}
