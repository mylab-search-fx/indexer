using MyLab.Search.Indexer.Application.Abstraction;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.Redis.Abstractions;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class RedisIndexLocker(IRedisService redis, ILogger<RedisIndexLocker>? logger = null) : IIndexLocker
{
    private readonly IDslLogger? _log = logger?.Dsl();
    
    public async Task<IIndexLock> TryLockAsync(string indexId, CancellationToken ct)
    {
        if (!redis.Connected)
        {
            _log?.Warning("Redis is not connected. No locking will occur.").Write();
            return new MockIndexLock();
        }

        var key = "indexer:" + indexId;

        var acquisition = await redis.Lock(key).TryAcquireOnceAsync(ct);

        return acquisition.Acquired 
            ? new RedisIndexLock(acquisition.Lease!) 
            : new UnlockedLock();
    }
}