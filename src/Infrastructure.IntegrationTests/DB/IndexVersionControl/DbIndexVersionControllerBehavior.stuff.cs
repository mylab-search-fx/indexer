using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.DB.IndexVersionControl;
using Xunit.Abstractions;

namespace Infrastructure.IntegrationTests.DB.IndexVersionControl;

public partial class DbIndexVersionControllerBehavior
{
    readonly TmpDbFixture _db;

    public DbIndexVersionControllerBehavior(ITestOutputHelper output, TmpDbFixture<VersionsTableInitializer> db)
    {
        db.Output = output;
        _db = db;
    }

    static async Task StoreVersionAsync(IDbManager db, string value, DateTime registered)
    {
        await using var connection = db.Use();
        var row = new IndexVersion
        {
            Value = value,
            Registered = registered
        };
        await connection.InsertAsync(row);
    }

    static async Task<IndexVersion[]> ReadVersionsAsync(IDbManager db)
    {
        await using var connection = db.Use();
        var table = connection.Tab<IndexVersion>();
        var query = table.OrderBy(row => row.Registered);
        var rows = await query.ToArrayAsync();
        return rows;
    }
}

public sealed class VersionsTableInitializer : ITestDbInitializer
{
    public Task InitializeAsync(DataConnection dataConnection)
    {
        return dataConnection.CreateTableAsync<IndexVersion>();
    }
}
