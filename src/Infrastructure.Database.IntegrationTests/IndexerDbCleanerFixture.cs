using LinqToDB;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

namespace Infrastructure.Database.IntegrationTests;

public class IndexerDbCleanerFixture : RemoteDbFixture
{
    public IndexerDbCleanerFixture() 
        : base(new MariaDb10DataProvider(), DbTestTools.CreateCs("indexer"))
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await CleanupAsync();
    }

    public override async Task DisposeAsync()
    {
        await CleanupAsync();
        await base.DisposeAsync();
    }

    public async Task CleanupAsync()
    {
        await using var c = Manager.Use();

        await c.Tab<IndexVersion>().DeleteAsync();
        await c.Tab<IndexSeed>().DeleteAsync();
    }
}