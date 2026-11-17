using MyLab.Redis.Options;
using MyLab.Search.Indexer.Infrastructure.Redis;
using Xunit.Abstractions;

namespace Infrastructure.IntegrationTests.Redis;

public class RedisIndexLockerBehavior(ITestOutputHelper output)
{
    [Fact(Timeout = 10000)]
    public async Task ShouldLockIndexWhenItIsFree()
    {
        // Arrange
        var opened = await TestTools.OpenRedisAsync(output);
        await using var redis = opened;
        var indexId = redis.NewIndexId();
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);

        // Act
        await using var indexLock = await locker.TryLockAsync(indexId, CancellationToken.None);

        var keyExists = await TestTools.LockExistsAsync(redis.Service, indexId);

        // Assert
        Assert.True(indexLock.Locked);
        Assert.IsType<RedisIndexLock>(indexLock);
        Assert.True(keyExists);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReturnUnlockedLockWhenIndexIsAlreadyHeld()
    {
        // Arrange
        var opened = await TestTools.OpenRedisAsync(output);
        await using var redis = opened;
        var indexId = redis.NewIndexId();
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);
        await using var held = await locker.TryLockAsync(indexId, CancellationToken.None);
        if (!held.Locked)
            throw new InvalidOperationException("Index was not locked.");

        // Act
        await using var second = await locker.TryLockAsync(indexId, CancellationToken.None);

        var keyExists = await TestTools.LockExistsAsync(redis.Service, indexId);

        // Assert
        Assert.False(second.Locked);
        Assert.IsType<UnlockedLock>(second);
        Assert.True(keyExists);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReleaseIndexWhenLockIsDisposed()
    {
        // Arrange
        var opened = await TestTools.OpenRedisAsync(output);
        await using var redis = opened;
        var indexId = redis.NewIndexId();
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);
        await using var held = await locker.TryLockAsync(indexId, CancellationToken.None);
        if (!held.Locked)
            throw new InvalidOperationException("Index was not locked.");

        // Act
        await held.DisposeAsync();

        var keyExists = await TestTools.LockExistsAsync(redis.Service, indexId);
        await using var again = await locker.TryLockAsync(indexId, CancellationToken.None);

        // Assert
        Assert.False(keyExists);
        Assert.True(again.Locked);
        Assert.IsType<RedisIndexLock>(again);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldLockAnotherIndexWhileOneIsHeld()
    {
        // Arrange
        var opened = await TestTools.OpenRedisAsync(output);
        await using var redis = opened;
        var firstId = redis.NewIndexId();
        var secondId = redis.NewIndexId();
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);
        await using var first = await locker.TryLockAsync(firstId, CancellationToken.None);
        if (!first.Locked)
            throw new InvalidOperationException("Index was not locked.");

        // Act
        await using var second = await locker.TryLockAsync(secondId, CancellationToken.None);

        var secondKeyExists = await TestTools.LockExistsAsync(redis.Service, secondId);
        await using var firstAgain = await locker.TryLockAsync(firstId, CancellationToken.None);

        // Assert
        Assert.True(second.Locked);
        Assert.IsType<RedisIndexLock>(second);
        Assert.True(secondKeyExists);
        Assert.False(firstAgain.Locked);
        Assert.IsType<UnlockedLock>(firstAgain);
    }

    [Fact(Timeout = 20000)]
    public async Task ShouldExtendLockWhenHeartbeatSucceeds()
    {
        // Arrange
        var opened = await TestTools.OpenRedisAsync(output, opt =>
        {
            TestTools.ConfigureRedis(opt);
            opt.Redlock = new RedlockOptions { ExpirySec = 5 };
        });
        await using var redis = opened;
        var indexId = redis.NewIndexId();
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);
        await using var held = await locker.TryLockAsync(indexId, CancellationToken.None);
        if (!held.Locked)
            throw new InvalidOperationException("Index was not locked.");
        await Task.Delay(TimeSpan.FromSeconds(1));
        var beforeTtl = await TestTools.LockTtlAsync(redis.Service, indexId);
        if (beforeTtl == null)
            throw new InvalidOperationException("Lock key has no TTL.");
        var before = beforeTtl.Value;

        // Act
        var prolonged = await held.HeartbeatAsync(CancellationToken.None);

        var after = await TestTools.LockTtlAsync(redis.Service, indexId);
        await using var second = await locker.TryLockAsync(indexId, CancellationToken.None);

        // Assert
        Assert.True(prolonged);
        Assert.NotNull(after);
        Assert.True(after.Value > before);
        Assert.False(second.Locked);
        Assert.IsType<UnlockedLock>(second);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReturnStandInLockWhenRedisIsNotConnected()
    {
        // Arrange
        var opened = await TestTools.OpenDisconnectedAsync(output);
        await using var redis = opened;
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);

        // Act
        await using var indexLock = await locker.TryLockAsync("orders", CancellationToken.None);

        var heartbeat = await indexLock.HeartbeatAsync(CancellationToken.None);

        // Assert
        Assert.True(indexLock.Locked);
        Assert.IsType<MockIndexLock>(indexLock);
        Assert.True(heartbeat);
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReturnAnotherStandInLockWhenRedisIsNotConnected()
    {
        // Arrange
        var opened = await TestTools.OpenDisconnectedAsync(output);
        await using var redis = opened;
        var locker = new RedisIndexLocker(redis.Service, redis.Logger);
        var first = await locker.TryLockAsync("orders", CancellationToken.None);
        if (!first.Locked)
            throw new InvalidOperationException("Stand-in lock was not returned.");

        // Act
        await using var second = await locker.TryLockAsync("orders", CancellationToken.None);

        var secondHeartbeat = await second.HeartbeatAsync(CancellationToken.None);
        var firstHeartbeat = await first.HeartbeatAsync(CancellationToken.None);
        await first.DisposeAsync();

        // Assert
        Assert.True(second.Locked);
        Assert.IsType<MockIndexLock>(second);
        Assert.True(secondHeartbeat);
        Assert.True(firstHeartbeat);
    }
}
