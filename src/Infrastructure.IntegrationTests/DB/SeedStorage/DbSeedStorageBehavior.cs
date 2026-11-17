using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.DB.SeedStorage;

namespace Infrastructure.IntegrationTests.DB.SeedStorage;

public partial class DbSeedStorageBehavior : IClassFixture<TmpDbFixture<SeedTableInitializer>>
{
    [Fact(Timeout = 10000)]
    public async Task ShouldReturnZeroWhenIndexHasNoSeed()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        var storage = new DbSeedStorage(db);

        // Act
        var seed = await storage.GetAsync("orders", CancellationToken.None);

        // Assert
        Assert.Equal(0, seed);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReturnSeedStoredForRequestedIndex()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        await StoreSeedAsync(db, "orders", 15);
        await StoreSeedAsync(db, "customers", 40);
        var storage = new DbSeedStorage(db);

        // Act
        var seed = await storage.GetAsync("orders", CancellationToken.None);

        // Assert
        Assert.Equal(15, seed);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldStoreSeedWhenIndexHasNone()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        await StoreSeedAsync(db, "customers", 40);
        var storage = new DbSeedStorage(db);

        // Act
        await storage.SetAsync("orders", 15, CancellationToken.None);

        var stored = await ReadSeedsAsync(db);

        // Assert
        Assert.Equal(2, stored.Length);
        Assert.Equal("customers", stored[0].IndexId);
        Assert.Equal(40, stored[0].Value);
        Assert.Equal("orders", stored[1].IndexId);
        Assert.Equal(15, stored[1].Value);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReplaceSeedWhenIndexAlreadyHasOne()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        await StoreSeedAsync(db, "customers", 40);
        await StoreSeedAsync(db, "orders", 15);
        var storage = new DbSeedStorage(db);

        // Act
        await storage.SetAsync("orders", 90, CancellationToken.None);

        var stored = await ReadSeedsAsync(db);

        // Assert
        Assert.Equal(2, stored.Length);
        Assert.Equal("customers", stored[0].IndexId);
        Assert.Equal(40, stored[0].Value);
        Assert.Equal("orders", stored[1].IndexId);
        Assert.Equal(90, stored[1].Value);
    }
}
