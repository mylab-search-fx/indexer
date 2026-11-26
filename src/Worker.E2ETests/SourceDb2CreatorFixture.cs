using Infrastructure.Database.IntegrationTests;
using LinqToDB.Data;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Worker.E2ETests;

public class SourceDb2CreatorFixture : RemoteDbFixture
{
    public SourceDb2CreatorFixture() :
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

        await c.ExecuteAsync($"DROP DATABASE IF EXISTS `{E2ETestTools.Source2DbName}`");
        await c.ExecuteAsync($"CREATE DATABASE `{E2ETestTools.Source2DbName}` CHARACTER SET utf8mb4");
    }

    public override async Task DisposeAsync()
    {
        await Manager.DoOnce().ExecuteAsync($"DROP DATABASE IF EXISTS `{E2ETestTools.Source2DbName}`");

        await base.DisposeAsync();
    }
}