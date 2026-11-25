using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.Database;
using Xunit.Abstractions;
using static Infrastructure.Database.IntegrationTests.SourceRepository.DbSourceRepositoryBehavior;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class DbSourceRepositoryBehavior : 
    IClassFixture<LocalServerDbFixture>, 
    IClassFixture<SourceDbFixture>,
    IAsyncLifetime
{
    const string TestDbName = "test_src";
    const int PageSize = 2;
    readonly IDbManager _srvDb;
    readonly IDbManager _srcDb;
    TestItem[] _items;

    static readonly IndexDescription IntSeedIndex = new 
    (
        "items",
        """
        SELECT id, val, last_modified, id as new_seed from item
        WHERE id > @seed
        ORDER BY id
        LIMIT @limit
        """,
        new IndexConfig(PageSize: PageSize)
    );

    static readonly IndexDescription DtSeedIndex = new 
    (
        "items",
        """
        SELECT id, val, last_modified, last_modified as new_seed from item
        WHERE last_modified >= @seed_dt
        ORDER BY last_modified
        LIMIT @limit
        """,
        new IndexConfig(PageSize: PageSize)
    );

    public DbSourceRepositoryBehavior(LocalServerDbFixture srvFxt, SourceDbFixture srcFxt, ITestOutputHelper output)
    {
        srvFxt.Output = output;
        srcFxt.Output = output;

        _srvDb = srvFxt.Manager;
        _srcDb = srcFxt.Manager;
    }

    public async Task InitializeAsync()
    {
        await using var c = _srvDb.Use();

        await c.ExecuteAsync($"DROP DATABASE IF EXISTS `{TestDbName}`");
        await c.ExecuteAsync($"CREATE DATABASE `{TestDbName}` CHARACTER SET utf8mb4");

        await _srcDb.DoOnce().CreateTableAsync<TestItem>();

        await FillDbAsync();
    }

    async Task FillDbAsync()
    {
        _items = Enumerable.Range(0, 4)
            .Select(i => new TestItem
            {
                Id = i,
                Value = "value-" +i,
                LastModified = DateTime.Now.AddSeconds(i)
            })
            .ToArray();

        await _srcDb.DoOnce().BulkCopyAsync(_items);
    }

    public Task DisposeAsync()
    {
        return _srvDb.DoOnce().ExecuteAsync($"DROP DATABASE IF EXISTS `{TestDbName}`");
    }

    static long DtToLongAndRound(DateTime dateTime)
        => dateTime.Ticks / 10_000_000 * 10_000_000;

    [Table("item")]
    public class TestItem
    {
        [Column("id", IsPrimaryKey = true)]
        public int Id { get; set; }
        [Column("val")]
        public string? Value { get; set; }
        [Column("last_modified")]
        public DateTime LastModified { get; set; }
    }

    public class SourceDbFixture : RemoteDbFixture
    {
        public SourceDbFixture(): 
            base
            (
                new MariaDb10DataProvider(), 
                TestTools.CreateCs(TestDbName)
            )
        {
            
        }
    }
}
