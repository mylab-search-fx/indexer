using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.IndexVersionControl;

public partial class DbIndexVersionControllerBehavior
{
    readonly ServiceProvider _app;
    readonly IDbManager _db;

    public DbIndexVersionControllerBehavior(ITestOutputHelper output)
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

    static Task<IndexVersion[]> ReadVersionsAsync(IDbManager db)
    {
        return db.DoOnce()
            .Tab<IndexVersion>()
            .OrderBy(row => row.Registered)
            .ToArrayAsync();
    }
}
