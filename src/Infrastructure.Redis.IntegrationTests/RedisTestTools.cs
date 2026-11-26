using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLab.Log;
using MyLab.Log.XUnit;
using MyLab.Redis;
using MyLab.Redis.Abstractions;
using MyLab.Redis.Options;
using MyLab.Search.Indexer.Infrastructure.Redis;
using Xunit.Abstractions;

namespace Infrastructure.Redis.IntegrationTests;

public static class RedisTestTools
{
    // Redis from test/docker-compose.yml: published port 16379, ACL user test.
    static RedisOptions RedisOptions => new()
    {
        ConnectionString = "localhost:16379",
        User = "test",
        Password = "test"
    };

    public static Action<RedisOptions> ConfigureLocalRedis => opt =>
    {
        opt.ConnectionString = RedisOptions.ConnectionString;
        opt.User = RedisOptions.User;
        opt.Password = RedisOptions.Password;
    };

    public static string LockKey(string indexId) => "redlock:indexer:" + indexId;

    public static async Task<bool> LockExistsAsync(IRedisService redis, string indexId)
    {
        var storedKey = redis.Db().String(LockKey(indexId));
        var exists = await storedKey.ExistsAsync();
        return exists;
    }

    public static async Task<TimeSpan?> LockTtlAsync(IRedisService redis, string indexId)
    {
        var storedKey = redis.Db().String(LockKey(indexId));
        var ttl = await storedKey.GetTtlAsync();
        return ttl;
    }

    public static async Task StartHostedAsync(IServiceProvider sp)
    {
        var hostedServices = sp.GetServices<IHostedService>().ToArray();
        foreach (var hostedService in hostedServices)
            await hostedService.StartAsync(CancellationToken.None);
    }

    public static async Task StopHostedAsync(IServiceProvider sp)
    {
        var hostedServices = sp.GetServices<IHostedService>().ToArray();
        foreach (var hostedService in hostedServices)
            await hostedService.StopAsync(CancellationToken.None);
    }

    public static IServiceCollection CreateApp(Action<RedisOptions> configureOptions, ITestOutputHelper output)
    {
        var services = new ServiceCollection();

        services
            .AddLogging(c => c
                .AddMyLabConsole()
                .AddXUnit(output))
            .AddRedis()
            .ConfigureRedis(configureOptions);

        return services;
    }

    public static Task<RedisSession> OpenRedisAsync(ITestOutputHelper output)
        => OpenRedisAsync(output, ConfigureLocalRedis);

    public static async Task<RedisSession> OpenRedisAsync(ITestOutputHelper output, Action<RedisOptions> configureOptions)
    {
        var provider = CreateApp(configureOptions, output).BuildServiceProvider();

        try
        {
            await StartHostedAsync(provider);

            var service = provider.GetRequiredService<IRedisService>();
            if (!service.Connected)
                throw new InvalidOperationException("Redis is not connected.");

            var logger = provider.GetRequiredService<ILogger<RedisIndexLocker>>();
            return new RedisSession(provider, service, logger);
        }
        catch
        {
            await StopHostedAsync(provider);
            await provider.DisposeAsync();
            throw;
        }
    }

    public static async Task<RedisSession> OpenDisconnectedAsync(ITestOutputHelper output)
    {
        var provider = CreateApp(_ => { }, output).BuildServiceProvider();

        try
        {
            await StartHostedAsync(provider);

            var service = provider.GetRequiredService<IRedisService>();
            if (service.Connected)
                throw new InvalidOperationException("Redis is connected.");

            var logger = provider.GetRequiredService<ILogger<RedisIndexLocker>>();
            return new RedisSession(provider, service, logger);
        }
        catch
        {
            await StopHostedAsync(provider);
            await provider.DisposeAsync();
            throw;
        }
    }
}

public sealed class RedisSession(
    ServiceProvider provider,
    IRedisService service,
    ILogger<RedisIndexLocker> logger) : IAsyncDisposable
{
    public IRedisService Service { get; } = service;

    public ILogger<RedisIndexLocker> Logger { get; } = logger;

    public string NewIndexId() => "idx-" + Guid.NewGuid().ToString("N");

    public async ValueTask DisposeAsync()
    {
        await RedisTestTools.StopHostedAsync(provider);
        await provider.DisposeAsync();
    }
}
