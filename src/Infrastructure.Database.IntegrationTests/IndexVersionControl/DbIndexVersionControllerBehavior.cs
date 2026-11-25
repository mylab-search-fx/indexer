using LinqToDB;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;

namespace Infrastructure.Database.IntegrationTests.IndexVersionControl;

public partial class DbIndexVersionControllerBehavior
{
    [Fact(Timeout = 10000)]
    public async Task ShouldAcceptVersionWhenItIsNotStoredYet()
    {
        // Arrange
        const string version = "script-alpha";
        var controller = new DbIndexVersionController(_db);

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync();

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
        const string version = "script-beta";
        var earlier = new DateTime(2020, 1, 1);

        await _db.DoOnce().InsertAsync(new IndexVersion
        {
            Value = version,
            Registered = earlier
        });
        
        var controller = new DbIndexVersionController(_db);
        

        // Act
        var accepted = await controller.CheckoutVersionAsync(version, CancellationToken.None);

        var stored = await ReadVersionsAsync();

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
        await _db.DoOnce().InsertAsync(new IndexVersion
        {
            Value = "script-alpha",
            Registered = new DateTime(2020, 1, 1)
        });

        await _db.DoOnce().InsertAsync(new IndexVersion
        {
            Value = "script-beta",
            Registered = new DateTime(2024, 6, 1)
        });

        var controller = new DbIndexVersionController(_db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-beta", CancellationToken.None);

        var stored = await ReadVersionsAsync();

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
        await _db.DoOnce().InsertAsync(new IndexVersion
        {
            Value = "script-alpha",
            Registered = new DateTime(2020, 1, 1)
        });

        await _db.DoOnce().InsertAsync(new IndexVersion
        {
            Value = "script-beta",
            Registered = new DateTime(2024, 6, 1)
        });

        var controller = new DbIndexVersionController(_db);

        // Act
        var accepted = await controller.CheckoutVersionAsync("script-alpha", CancellationToken.None);

        var stored = await ReadVersionsAsync();

        // Assert
        Assert.False(accepted);
        Assert.Equal(2, stored.Length);
        Assert.Equal("script-alpha", stored[0].Value);
        Assert.Equal("script-beta", stored[1].Value);
    }
}
