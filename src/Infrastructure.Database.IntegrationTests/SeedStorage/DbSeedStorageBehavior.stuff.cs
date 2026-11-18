using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SeedStorage;

public partial class DbSeedStorageBehavior
{
    readonly ServiceProvider _app;
    readonly IDbManager _db;

    public DbSeedStorageBehavior(ITestOutputHelper output)
    {
        _app = TestTools.CreateApp(output).BuildServiceProvider();
        _db = _app.GetRequiredService<IDbManager>();
    }

    public async Task InitializeAsync()
    {
        await TestTools.ClearTestTablesAsync(_db);
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    static async Task StoreSeedAsync(IDbManager db, string indexId, long value)
    {
        await using var connection = db.Use();
        var row = new IndexSeed
        {
            IndexId = indexId,
            Value = value
        };
        await connection.InsertAsync(row);
    }

    static Task<IndexSeed[]> ReadSeedsAsync(IDbManager db)
    {
        return db.DoOnce()
            .Tab<IndexSeed>()
            .OrderBy(row => row.IndexId)
            .ToArrayAsync();
    }
}
