using LinqToDB.Data;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Infrastructure.Database.IntegrationTests;

public class SourceDbCreatorFixture : RemoteDbFixture
{
    public SourceDbCreatorFixture() :
        base
        (
            new MariaDb10DataProvider(),
            DbTestTools.CreateCs()
        )
    {

    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await using var c = Manager.Use();

        await c.ExecuteAsync($"DROP DATABASE IF EXISTS `{DbTestTools.SourceDbName}`");
        await c.ExecuteAsync($"CREATE DATABASE `{DbTestTools.SourceDbName}` CHARACTER SET utf8mb4");
    }

    public override async Task DisposeAsync()
    {
        await Manager.DoOnce().ExecuteAsync($"DROP DATABASE IF EXISTS `{DbTestTools.SourceDbName}`");

        await base.DisposeAsync();
    }
}