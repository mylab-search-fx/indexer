namespace Worker.E2ETests;

public partial class WorkerBehavior
{
    [Fact]
    public async Task ShouldIndexInitialData()
    {
        //Arrange
        var items = await WriteItemsAsync(0, 100);

        //Act
        await using var disp = await StartAppAsync();

        bool allItemsIndexed = await CheckIndexedItemsAsync(items);

        //Assert
        Assert.True(allItemsIndexed);
    }

    [Fact]
    public async Task ShouldIndexAdditionalDataOnExistentData()
    {
        //Arrange
        await WriteItemsAsync(0, 100);
        await using var disp = await StartAppAsync();

        //Act
        var items = await WriteItemsAsync(100, 100);

        bool allItemsIndexed = await CheckIndexedItemsAsync(items);

        //Assert
        Assert.True(allItemsIndexed);
    }

    [Fact]
    public async Task  ShouldIndexAdditionalDataOnEmptyDb()
    {
        //Arrange
        await using var disp = await StartAppAsync();

        //Act
        var items = await WriteItemsAsync(0, 100);

        bool allItemsIndexed = await CheckIndexedItemsAsync(items);

        //Assert
        Assert.True(allItemsIndexed);
    }
}