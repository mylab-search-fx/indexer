using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.DB.IndexVersionControl;

namespace Infrastructure.IntegrationTests.DB.IndexVersionControl;

public partial class DbIndexVersionControllerBehavior : IClassFixture<TmpDbFixture<VersionsTableInitializer>>
{
    [Fact(Timeout = 10000)]
    public async Task ShouldAcceptVersionWhenItIsNotStoredYet()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        var controller = new DbIndexVersionController(db);
        var version = "script-alpha";

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync(db);

        // Assert
        Assert.True(accepted);
        var row = Assert.Single(stored);
        Assert.Equal(version, row.Value);
        Assert.NotEqual(default, row.Registered);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldAcceptAndStoreVersionWhenOtherVersionsExist()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        var earlier = new DateTime(2020, 1, 1);
        await StoreVersionAsync(db, "script-alpha", earlier);
        var controller = new DbIndexVersionController(db);
        var version = "script-beta";

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync(db);

        // Assert
        Assert.True(accepted);
        Assert.Equal(2, stored.Length);
        Assert.Equal("script-alpha", stored[0].Value);
        Assert.Equal(earlier, stored[0].Registered);
        Assert.Equal(version, stored[1].Value);
        Assert.True(stored[1].Registered > earlier);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldAcceptVersionWhenItIsTheLatestStored()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        await StoreVersionAsync(db, "script-alpha", new DateTime(2020, 1, 1));
        await StoreVersionAsync(db, "script-beta", new DateTime(2024, 6, 1));
        var controller = new DbIndexVersionController(db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-beta", CancellationToken.None);

        var stored = await ReadVersionsAsync(db);

        // Assert
        Assert.True(accepted);
        Assert.Equal(2, stored.Length);
        Assert.Equal("script-alpha", stored[0].Value);
        Assert.Equal("script-beta", stored[1].Value);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldRejectVersionWhenANewerOneIsStored()
    {
        // Arrange
        var db = await _db.CreateDbAsync();
        await StoreVersionAsync(db, "script-alpha", new DateTime(2020, 1, 1));
        await StoreVersionAsync(db, "script-beta", new DateTime(2024, 6, 1));
        var controller = new DbIndexVersionController(db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-alpha", CancellationToken.None);

        var stored = await ReadVersionsAsync(db);

        // Assert
        Assert.False(accepted);
        Assert.Equal(2, stored.Length);
        Assert.Equal("script-alpha", stored[0].Value);
        Assert.Equal("script-beta", stored[1].Value);
    }
}
