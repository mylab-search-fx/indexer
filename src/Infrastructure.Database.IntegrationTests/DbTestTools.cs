using LinqToDB;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Config;

namespace Infrastructure.Database.IntegrationTests;

public static class DbTestTools
{
    public const string SourceDbname = "test_src";

    public static IndexDescription CreateIntSeedIndex(int pageSize) => new
    (
        "items",
        """
        SELECT id, val, last_modified, id as new_seed from item
        WHERE id > @seed
        ORDER BY id
        LIMIT @limit
        """,
        new IndexConfig(PageSize: pageSize)
    );

    public static IndexDescription CreateDtSeedIndex(int pageSize) => new
    (
        "items",
        """
        SELECT id, val, last_modified, last_modified as new_seed from item
        WHERE last_modified >= @seed_dt
        ORDER BY last_modified
        LIMIT @limit
        """,
        new IndexConfig(PageSize: pageSize)
    );

    public static string CreateCs(string? database = null)
    {
        const string cs = "Server=127.0.0.1;Port=13306;User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";
        return database != null
            ? cs + ";Database=" + database
            : cs;
    }

    public static async Task CreateTestDbAsync(IDbManager srvDb)
    {
        await using var c = srvDb.Use();

        await c.ExecuteAsync($"DROP DATABASE IF EXISTS `{DbTestTools.SourceDbname}`");
        await c.ExecuteAsync($"CREATE DATABASE `{DbTestTools.SourceDbname}` CHARACTER SET utf8mb4");
    }

    public static Task DropTestDbAsync(IDbManager srvDb)
    {
        return srvDb.DoOnce().ExecuteAsync($"DROP DATABASE IF EXISTS `{DbTestTools.SourceDbname}`");
    }

    public static Task CreateTestTableAsync(IDbManager db)
        => db.DoOnce().CreateTableAsync<TestDbItem>();

    public static Task DropTestTableAsync(IDbManager db)
        => db.DoOnce().Tab<TestDbItem>().DeleteAsync();
}