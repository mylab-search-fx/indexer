using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class RedlockIndexLocker : IIndexLocker, IDisposable
{
    readonly RedLockFactory? _redlockFactory;
    readonly TimeSpan _expiry;
    
    public RedlockIndexLocker(IOptions<WorkerRedisOptions> opts)
    {
        if (opts.Value == null)
        {
            _redlockFactory = null;
            return;
        }

        if (opts.Value.Host == null)
            throw new InvalidOperationException("Wrong Redis host");

        if (opts.Value.RedlockExpirySec <= 0)
            throw new InvalidOperationException("Wrong RedlockExpirySec");

        _expiry = TimeSpan.FromSeconds(opts.Value.RedlockExpirySec);

        var config = new ConfigurationOptions
        {
            EndPoints = { { "redis-host", 6379 } },
            User = "your-username",      // Имя пользователя Redis ACL
            Password = "your-password",  // Пароль
            Ssl = true                   // При необходимости
        };

        var multiplexer = ConnectionMultiplexer.Connect(config);
        var multiplexers = new List<RedLockMultiplexer> { multiplexer };

        _redlockFactory = RedLockFactory.Create(endPoints);
    }
    
    public async Task<IIndexLock?> TryLockAsync(string indexId, CancellationToken ct)
    {
        if (_redlockFactory == null)
        {
            return new MockIndexLock();
        }
        
        var key = "indexer:" + indexId;

        var redlock = await _redlockFactory.CreateLockAsync(key, _expiry);

        if (redlock is { IsAcquired: true })
        {
            return new RedlockIndexLock(redlock);
        }

        return null;
    }

    public void Dispose()
    {
        _redlockFactory?.Dispose();
    }
}