using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class MockIndexLock : IIndexLock
{
    public bool Locked => true;

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task<bool> HeartbeatAsync(CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}