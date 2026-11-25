using LinqToDB;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

namespace Infrastructure.Database.IntegrationTests.SeedStorage;

public partial class DbSeedStorageBehavior
{
    [Fact(Timeout = 10000)]
    public async Task ShouldReturnZeroWhenIndexHasNoSeed()
    {
        // Arrange
        var storage = new DbSeedStorage(_db);

        // Act
        var seed = await storage.GetAsync("orders", CancellationToken.None);

        // Assert
        Assert.Equal(0, seed);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReturnSeedStoredForRequestedIndex()
    {
        // Arrange
        await _db.DoOnce().InsertAsync(new IndexSeed
        {
            IndexId = "orders",
            Value = 15
        });

        await _db.DoOnce().InsertAsync(new IndexSeed
        {
            IndexId = "customers",
            Value = 40
        });

        var storage = new DbSeedStorage(_db);

        // Act
        var seed = await storage.GetAsync("orders", CancellationToken.None);

        // Assert
        Assert.Equal(15, seed);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldStoreSeedWhenIndexHasNone()
    {
        // Arrange
        await _db.DoOnce().InsertAsync(new IndexSeed
        {
            IndexId = "customers",
            Value = 40
        });

        var storage = new DbSeedStorage(_db);

        // Act
        await storage.SetAsync("orders", 15, CancellationToken.None);

        var stored = await ReadSeedsAsync(_db);

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
        await _db.DoOnce().InsertAsync(new IndexSeed
        {
            IndexId = "orders",
            Value = 15
        });

        await _db.DoOnce().InsertAsync(new IndexSeed
        {
            IndexId = "customers",
            Value = 40
        });

        var storage = new DbSeedStorage(_db);

        // Act
        await storage.SetAsync("orders", 90, CancellationToken.None);

        var stored = await ReadSeedsAsync(_db);

        // Assert
        Assert.Equal(2, stored.Length);
        Assert.Equal("customers", stored[0].IndexId);
        Assert.Equal(40, stored[0].Value);
        Assert.Equal("orders", stored[1].IndexId);
        Assert.Equal(90, stored[1].Value);
    }
}
