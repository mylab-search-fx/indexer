using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.DB.SeedStorage;
using Xunit.Abstractions;

namespace Infrastructure.IntegrationTests.DB.SeedStorage;

public partial class DbSeedStorageBehavior
{
    readonly TmpDbFixture _db;

    public DbSeedStorageBehavior(ITestOutputHelper output, TmpDbFixture<SeedTableInitializer> db)
    {
        db.Output = output;
        _db = db;
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

    static async Task<IndexSeed[]> ReadSeedsAsync(IDbManager db)
    {
        await using var connection = db.Use();
        var table = connection.Tab<IndexSeed>();
        var query = table.OrderBy(row => row.IndexId);
        var rows = await query.ToArrayAsync();
        return rows;
    }
}

public sealed class SeedTableInitializer : ITestDbInitializer
{
    public Task InitializeAsync(DataConnection dataConnection)
    {
        return dataConnection.CreateTableAsync<IndexSeed>();
    }
}
