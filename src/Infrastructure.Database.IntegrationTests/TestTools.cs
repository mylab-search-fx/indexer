using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Db;
using MyLab.Log;
using MyLab.Log.XUnit;
using MyLab.Search.Indexer.Infrastructure.Database;
using MySqlConnector;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests;

static class TestTools
{
    // Local MariaDB, user test / password test.
    public const int Port = 13306;

    public const string IndexerDatabase = "indexer_it";

    public const string SourceDatabase = "indexer_source_it";

    public const string SourceConnection = "Source";

    public const string ServerConnection = "Server";

    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = DatabaseConnectionString(IndexerDatabase),
            ["ConnectionStrings:Source"] = DatabaseConnectionString(SourceDatabase),
            ["ConnectionStrings:Server"] = ServerConnectionString()
        })
        .Build();

    public static IServiceCollection CreateApp(ITestOutputHelper output) =>
        CreateApp(logging => logging
            .AddMyLabConsole()
            .AddXUnit(output));

    public static IServiceCollection CreateApp() =>
        CreateApp(logging => logging.AddMyLabConsole());

    public static async Task ClearTestTablesAsync(IDbManager db)
    {
        await ClearDatabaseAsync(db);
        await ClearDatabaseAsync(db, SourceConnection);
    }

    static IServiceCollection CreateApp(Action<ILoggingBuilder> configureLogging)
    {
        var services = new ServiceCollection();

        services
            .AddLogging(configureLogging)
            .AddDbTools(new SingleDbProviderSource(new MariaDb10DataProvider()))
            .ConfigureDbTools(Configuration);

        return services;
    }

    static async Task ClearDatabaseAsync(IDbManager db, string? connectionName = null)
    {
        await using var connection = connectionName == null
            ? db.Use()
            : db.Use(connectionName);

        var tables = await connection.QueryToArrayAsync<string>(
            """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_type = 'BASE TABLE'
            """);

        if (tables.Length == 0)
            return;

        await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0");

        foreach (var table in tables)
            await connection.ExecuteAsync($"DELETE FROM `{table}`");

        await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 1");
    }

    static string DatabaseConnectionString(string database) =>
        $"Server=127.0.0.1;Port={Port};Database={database};User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";

    static string ServerConnectionString() =>
        $"Server=127.0.0.1;Port={Port};User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";

    public static void ClearPools() => MySqlConnection.ClearAllPools();
}
