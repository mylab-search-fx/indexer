using LinqToDB.Async;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.IndexVersionControl;

[Collection("indexer-db")]
public partial class DbIndexVersionControllerBehavior : IClassFixture<IndexerDbFixture>, IAsyncLifetime
{
    readonly IDbManager _db;
    readonly IndexerDbFixture _fxt;

    public DbIndexVersionControllerBehavior(IndexerDbFixture fxt, ITestOutputHelper output)
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
