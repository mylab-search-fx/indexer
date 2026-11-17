using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class UnlockedLock : IIndexLock
{
    public bool Locked => false;
    public Task<bool> HeartbeatAsync(CancellationToken ct)
    {
        throw new InvalidOperationException("Not connected");
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}