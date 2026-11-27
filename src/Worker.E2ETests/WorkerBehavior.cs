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
        await WaitingForIndexer();

        var indexedItems = await ReadIndexedItemsAsync(items);
        _output.WriteLine("Items found: " + indexedItems.Count);

        var found = FoundCount(items, indexedItems);

        //Assert
        Assert.Equal(items.Count, found);
    }

    [Fact]
    public async Task ShouldIndexAdditionalDataOnExistentData()
    {
        //Arrange
        await WriteItemsAsync(0, 100);
        await using var disp = await StartAppAsync();
        await WaitingForIndexer();

        //Act
        var items = await WriteItemsAsync(100, 100);
        await WaitingForIndexer();

        var indexedItems = await ReadIndexedItemsAsync(items);
        _output.WriteLine("Items found: " + indexedItems.Count);

        var found = FoundCount(items, indexedItems);

        //Assert
        Assert.Equal(items.Count, found);
    }

    [Fact]
    public async Task  ShouldIndexAdditionalDataOnEmptyDb()
    {
        //Arrange
        await using var disp = await StartAppAsync();

        //Act
        var items = await WriteItemsAsync(0, 100);
        await WaitingForIndexer();

        var indexedItems = await ReadIndexedItemsAsync(items);
        _output.WriteLine("Items found: " + indexedItems.Count);

        var found = FoundCount(items, indexedItems);

        //Assert
        Assert.Equal(items.Count, found);
    }
}