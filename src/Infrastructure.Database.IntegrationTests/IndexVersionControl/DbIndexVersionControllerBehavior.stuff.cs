using LinqToDB.Async;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.IndexVersionControl;

[Collection("indexer-db")]
public partial class DbIndexVersionControllerBehavior : IClassFixture<IndexerDbCleanerFixture>, IAsyncLifetime
{
    readonly IDbManager _db;
    readonly IndexerDbCleanerFixture _fxt;

    public DbIndexVersionControllerBehavior(IndexerDbCleanerFixture fxt, ITestOutputHelper output)
    {
        fxt.Output = output;
        _db = fxt.Manager;
        _fxt = fxt;
    }

    Task<IndexVersion[]> ReadVersionsAsync()
    {
        return _db.DoOnce()
            .Tab<IndexVersion>()
            .OrderBy(row => row.Registered)
            .ToArrayAsync();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _fxt.CleanupAsync();
    }
}
