using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

namespace Infrastructure.Database.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<GlobalTestFixture>
{
    public const string Name = "Database";
}

public sealed class GlobalTestFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var app = TestTools.CreateApp().BuildServiceProvider();
        var db = app.GetRequiredService<IDbManager>();

        TestTools.ClearPools();

        await using (var server = db.Use(TestTools.ServerConnection))
        {
            await server.ExecuteAsync($"DROP DATABASE IF EXISTS `{TestTools.IndexerDatabase}`");
            await server.ExecuteAsync(
                $"CREATE DATABASE `{TestTools.IndexerDatabase}` CHARACTER SET utf8mb4");
            await server.ExecuteAsync($"DROP DATABASE IF EXISTS `{TestTools.SourceDatabase}`");
            await server.ExecuteAsync(
                $"CREATE DATABASE `{TestTools.SourceDatabase}` CHARACTER SET utf8mb4");
            await GrantTestUserAsync(server, TestTools.IndexerDatabase);
            await GrantTestUserAsync(server, TestTools.SourceDatabase);
        }

        await using var indexer = db.Use();
        await indexer.CreateTableAsync<IndexSeed>();
        await indexer.CreateTableAsync<IndexVersion>();
    }

    public async Task DisposeAsync()
    {
        await using var app = TestTools.CreateApp().BuildServiceProvider();
        var db = app.GetRequiredService<IDbManager>();

        TestTools.ClearPools();

        await using var server = db.Use(TestTools.ServerConnection);
        await server.ExecuteAsync($"DROP DATABASE IF EXISTS `{TestTools.IndexerDatabase}`");
        await server.ExecuteAsync($"DROP DATABASE IF EXISTS `{TestTools.SourceDatabase}`");
    }

    static Task GrantTestUserAsync(DataConnection server, string database) =>
        server.ExecuteAsync(
            $"GRANT ALL PRIVILEGES ON `{database}`.* TO 'test'@'%'");
}
