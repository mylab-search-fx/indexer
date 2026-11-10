namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

public partial class IndexWorkerBehavior
{
    [Fact(Timeout = 5000)]
    public async Task ShouldStopBeforeReadingWhenVersionChanges()
    {
        // Arrange
        var fixture = new Fixture { Version = "rev-7" };
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Same(fixture.Index, Assert.Single(fixture.VersionRequests));
        Assert.Equal(["rev-7"], fixture.CheckedOutVersions);
        Assert.Empty(fixture.LockedIndexes);
        Assert.Empty(fixture.ReadSeeds);
        Assert.Empty(fixture.WrittenPages);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldSkipReadingAndWritingWhenIndexIsBusy()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.AllowIterations(1);
        fixture.RefuseNextLock();
        fixture.EnqueuePage(count: 1, full: false);
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([fixture.Index.Name], fixture.LockedIndexes);
        Assert.Empty(fixture.ReadSeeds);
        Assert.Empty(fixture.WrittenPages);
        Assert.Equal(0, fixture.DisposedLocks);
        Assert.Equal(2, fixture.CheckedOutVersions.Count);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldReadOnNextIterationAfterIndexWasBusy()
    {
        // Arrange
        var fixture = new Fixture { Seed = 5 };
        fixture.AllowIterations(2);
        fixture.RefuseNextLock();
        var page = fixture.EnqueuePage(count: 1, full: false);
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([fixture.Index.Name, fixture.Index.Name], fixture.LockedIndexes);
        Assert.Equal([5L], fixture.ReadSeeds);
        Assert.Equal([page], fixture.WrittenPages);
        Assert.Equal(1, fixture.DisposedLocks);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldPassUpdatedSeedToNextPage()
    {
        // Arrange
        var fixture = new Fixture { Seed = 10 };
        fixture.AllowIterations(1);
        var first = fixture.EnqueuePage(count: 2, full: true, newSeed: 20);
        var second = fixture.EnqueuePage(count: 1, full: false, newSeed: 35);
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([fixture.Index.Name], fixture.LoadedSeedIndexes);
        Assert.Equal([10L, 20L], fixture.ReadSeeds);
        Assert.Equal([20L, 35L], fixture.SavedSeeds);
        Assert.Equal([fixture.Index.Name, fixture.Index.Name], fixture.SavedSeedIndexes);
        Assert.Equal([first, second], fixture.WrittenPages);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldReadNextPageWithStoredSeedWhenNewSeedIsMissing()
    {
        // Arrange
        var fixture = new Fixture { Seed = 10 };
        fixture.AllowIterations(1);
        var first = fixture.EnqueuePage(count: 4, full: true);
        var second = fixture.EnqueuePage(count: 1, full: false);
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([10L, 10L], fixture.ReadSeeds);
        Assert.Empty(fixture.SavedSeeds);
        Assert.Equal([first, second], fixture.WrittenPages);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldReadEveryFullPageAndStopWhenPageIsNotFull()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.AllowIterations(1);
        var first = fixture.EnqueuePage(count: 3, full: true);
        var second = fixture.EnqueuePage(count: 3, full: true);
        var shortPage = fixture.EnqueuePage(count: 1, full: false);
        fixture.EnqueuePage(count: 3, full: true);
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([first, second, shortPage], fixture.WrittenPages);
        Assert.Equal(3, fixture.ReadSeeds.Count);
        Assert.Equal(3, fixture.HeartbeatCount);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldStopPagingAndReleaseLockWhenHeartbeatFails()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.AllowIterations(1);
        var first = fixture.EnqueuePage(count: 3, full: true);
        fixture.EnqueuePage(count: 3, full: true);
        fixture.FailNextHeartbeat();
        var worker = fixture.CreateWorker();

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert
        Assert.Equal([first], fixture.WrittenPages);
        Assert.Single(fixture.ReadSeeds);
        Assert.Equal(1, fixture.HeartbeatCount);
        Assert.Equal(1, fixture.DisposedLocks);
    }

    [Fact(Timeout = 5000)]
    public async Task ShouldReleaseLockAndStopWorkerWhenWriteFails()
    {
        // Arrange
        var writeError = new InvalidOperationException("write failed");
        var fixture = new Fixture();
        fixture.AllowIterations(2);
        fixture.EnqueuePage(count: 2, full: true);
        fixture.EnqueuePage(count: 1, full: false);
        fixture.FailWrite(writeError);
        var worker = fixture.CreateWorker();

        // Act
        var error = await Record.ExceptionAsync(() => worker.RunAsync(CancellationToken.None));

        // Assert
        Assert.Same(writeError, error);
        Assert.Empty(fixture.WrittenPages);
        Assert.Single(fixture.ReadSeeds);
        Assert.Equal(0, fixture.HeartbeatCount);
        Assert.Equal(1, fixture.DisposedLocks);
        Assert.Single(fixture.CheckedOutVersions);
    }
}
