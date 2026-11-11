using MyLab.Search.Indexer.Application.Abstraction;
using RedLockNet;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class RedlockIndexLock(IRedLock redLock) : IIndexLock
{
    public async ValueTask DisposeAsync()
    {
        await redLock.DisposeAsync();
    }

    public Task<bool> HeartbeatAsync(CancellationToken ct)
    {
        return Task.FromResult(redLock.IsAcquired);
    }
}