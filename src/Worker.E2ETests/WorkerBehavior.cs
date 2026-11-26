namespace Worker.E2ETests;

public partial class WorkerBehavior
{
    [Fact]
    public async Task ShouldIndexInitialData()
    {
        //Arrange
        await WriteItemsAsync(0, 100);

        //Act
        await using var disp = await StartAppAsync();

        //Assert

    }

    [Fact]
    public async Task ShouldIndexAdditionalDataOnExistentData()
    {
        //Arrange
        await WriteItemsAsync(0, 100);
        await using var disp = await StartAppAsync();

        //Act
        await WriteItemsAsync(100, 100);

        //Assert

    }

    [Fact]
    public async Task  ShouldIndexAdditionalDataOnEmptyDb()
    {
        //Arrange
        await using var disp = await StartAppAsync();

        //Act
        await WriteItemsAsync(0, 100);

        //Assert

    }
}