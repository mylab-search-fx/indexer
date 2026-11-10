using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Indexing;
using Xunit.Abstractions;

namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

public partial class IndexWorkerOrchestratorBehavior(ITestOutputHelper output)
{
    [Fact(Timeout = 15000)]
    public async Task ShouldRunMoreThanTreadPoolWorkersInParallel()
    {
        // Arrange
        ThreadPool.GetMinThreads(out var minWorkerThreads, out _);

        output.WriteLine("Min worker threads: " + minWorkerThreads);

        var workerCount = minWorkerThreads * 2;
        var gate = new OverlapGate(workerCount);
        var descriptions = Enumerable.Range(0, workerCount)
            .Select(index => new IndexDescription($"worker-{index}", "select 1", new IndexConfig(Idle: 0)))
            .ToArray();
        var orchestrator = new IndexWorkerOrchestrator(
            new IndexWorkerFactory(new FixedIndexDescProvider(descriptions), new WorkerServices(gate)));
        using var cancellation = new CancellationTokenSource();
        using var overlapTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var running = orchestrator.RunAsync(cancellation.Token);
        var overlapAttempt = await Task.WhenAny(
            gate.AllEntered,
            Task.Delay(Timeout.Infinite, overlapTimeout.Token));
        await cancellation.CancelAsync();
        await running.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.Same(gate.AllEntered, overlapAttempt);
        Assert.Equal(workerCount, gate.MaxInFlight);
    }
}
