using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;

namespace Infrastructure.Database.IntegrationTests.IndexVersionControl;

[Collection(DatabaseCollection.Name)]
public partial class DbIndexVersionControllerBehavior : IAsyncLifetime
{
    [Fact(Timeout = 10000)]
    public async Task ShouldAcceptVersionWhenItIsNotStoredYet()
    {
        // Arrange
        var controller = new DbIndexVersionController(_db);
        var version = "script-alpha";

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync(_db);

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
        var earlier = new DateTime(2020, 1, 1);
        await StoreVersionAsync(_db, "script-alpha", earlier);
        var controller = new DbIndexVersionController(_db);
        var version = "script-beta";

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync(_db);

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
        await StoreVersionAsync(_db, "script-alpha", new DateTime(2020, 1, 1));
        await StoreVersionAsync(_db, "script-beta", new DateTime(2024, 6, 1));
        var controller = new DbIndexVersionController(_db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-beta", CancellationToken.None);

        var stored = await ReadVersionsAsync(_db);

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
        await StoreVersionAsync(_db, "script-alpha", new DateTime(2020, 1, 1));
        await StoreVersionAsync(_db, "script-beta", new DateTime(2024, 6, 1));
        var controller = new DbIndexVersionController(_db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-alpha", CancellationToken.None);

        var stored = await ReadVersionsAsync(_db);

        // Assert
        Assert.False(accepted);
        Assert.Equal(2, stored.Length);
        Assert.Equal("script-alpha", stored[0].Value);
        Assert.Equal("script-beta", stored[1].Value);
    }
}
