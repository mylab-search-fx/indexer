using LinqToDB;
using LinqToDB.SchemaProvider;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Config;

namespace Infrastructure.Database.IntegrationTests;

public static class DbTestTools
{
    public const string SourceDbName = "test_src";

    public static IndexDescription CreateIntSeedIndex(IndexConfig cfg) => new
    (
        "items",
        """
        SELECT id, val, last_modified, id as new_seed from item
        WHERE id > @seed
        ORDER BY id
        LIMIT @limit
        """,
        cfg
    );

    public static IndexDescription CreateDtSeedIndex(IndexConfig cfg) => new
    (
        "items",
        """
        SELECT id, val, last_modified, last_modified as new_seed from item
        WHERE last_modified >= @seed_dt
        ORDER BY last_modified
        LIMIT @limit
        """,
        cfg
    );

    public static string CreateCs(string? database = null)
    {
        const string cs = "Server=127.0.0.1;Port=13306;User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";
        return database != null
            ? cs + ";Database=" + database
            : cs;
    }

    public static async Task CreateOrResetTestTableAsync(IDbManager db)
    {
        await using var c = db.Use();

        var schema = c.DataProvider.GetSchemaProvider().GetSchema(c, new GetSchemaOptions
        {
            GetProcedures = false,
            GetTables = true
        });

        bool tableExists = schema.Tables.Any(t =>
            t.TableName?.Equals("item", StringComparison.OrdinalIgnoreCase) ?? false);

        if(tableExists)
            await c.DropTableAsync<TestDbItem>();
        
        await c.CreateTableAsync<TestDbItem>();
    }

    public static Task DropTestTableAsync(IDbManager db)
        => db.DoOnce().DropTableAsync<TestDbItem>();
}