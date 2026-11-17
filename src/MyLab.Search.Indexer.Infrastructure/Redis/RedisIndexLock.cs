using MyLab.Redis.Locking;
using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class RedisIndexLock(IRedisLockLease lease) : IIndexLock
{
    public bool Locked => true;

    public Task<bool> HeartbeatAsync(CancellationToken ct)
    {
        return lease.ProlongAsync();
    }

    public ValueTask DisposeAsync()
    {
        return lease.DisposeAsync();
    }
}