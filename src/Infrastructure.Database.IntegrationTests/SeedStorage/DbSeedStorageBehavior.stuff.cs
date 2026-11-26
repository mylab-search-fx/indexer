using LinqToDB.Async;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SeedStorage;

[Collection("indexer-db")]
public partial class DbSeedStorageBehavior : IClassFixture<IndexerDbCleanerFixture>, IAsyncLifetime
{
    readonly IDbManager _db;
    readonly IndexerDbCleanerFixture _fxt;

    public DbSeedStorageBehavior(IndexerDbCleanerFixture fxt, ITestOutputHelper output)
    {
        fxt.Output = output;
        _db = fxt.Manager;
        _fxt = fxt;
    }

    static Task<IndexSeed[]> ReadSeedsAsync(IDbManager db)
    {
        return db.DoOnce()
            .Tab<IndexSeed>()
            .OrderBy(row => row.IndexId)
            .ToArrayAsync();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _fxt.CleanupAsync();
    }
}
