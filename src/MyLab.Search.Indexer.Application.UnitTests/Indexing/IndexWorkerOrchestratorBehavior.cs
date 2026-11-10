using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Indexing;
using Xunit.Abstractions;

namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

public class IndexWorkerOrchestratorBehavior(ITestOutputHelper output)
{
    
    [Fact(Timeout = 15000)]
    public async Task ShouldRunMoreThanFourWorkersInParallel()
    {
        // Arrange
        ThreadPool.GetMinThreads(out var minWorkerThreads, out _);

        output.WriteLine("Min worker threads: " + minWorkerThreads);
        
        var workerCount = minWorkerThreads * 2;

        var inFlight = 0;
        var maxInFlight = 0;
        var allWorkersEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var workers = Enumerable.Range(0, workerCount)
            .Select(index => new StubIndexWorker($"worker-{index}", async cancellationToken =>
            {
                var current = Interlocked.Increment(ref inFlight);

                int priorPeak;
                while ((priorPeak = Volatile.Read(ref maxInFlight)) < current)
                {
                    if (Interlocked.CompareExchange(ref maxInFlight, current, priorPeak) == priorPeak)
                        break;
                }

                if (current == workerCount)
                    allWorkersEntered.TrySetResult();

                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                finally
                {
                    Interlocked.Decrement(ref inFlight);
                }
            }))
            .ToArray();

        var orchestrator = new IndexWorkerOrchestrator(new StubIndexWorkerFactory(workers));
        using var cancellation = new CancellationTokenSource();
        using var overlapTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var running = orchestrator.RunAsync(cancellation.Token);
        var overlapAttempt = await Task.WhenAny(
            allWorkersEntered.Task,
            Task.Delay(Timeout.Infinite, overlapTimeout.Token));
        await cancellation.CancelAsync();
        await running.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.Same(allWorkersEntered.Task, overlapAttempt);
        Assert.Equal(workerCount, Volatile.Read(ref maxInFlight));
    }

    sealed class StubIndexWorker(string name, Func<CancellationToken, Task> run) : IIndexWorker
    {
        public string Name { get; } = name;

        public Task RunAsync(CancellationToken ct) => run(ct);
    }

    sealed class StubIndexWorkerFactory(IReadOnlyList<IIndexWorker> workers) : IIndexWorkerFactory
    {
        public Task<IReadOnlyList<IIndexWorker>> CreateAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(workers);
    }
}
