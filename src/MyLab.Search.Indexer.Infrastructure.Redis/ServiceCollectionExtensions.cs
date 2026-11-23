using Microsoft.Extensions.Configuration;
using MyLab.Redis;
using MyLab.Redis.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Infrastructure.Redis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddRedisInfrastructure()
            => srv
                .AddSingleton<IIndexLocker, RedisIndexLocker>()
                .AddRedis();

        public IServiceCollection ConfigureRedisInfrastructure(IConfiguration cfg)
            => srv
                .ConfigureRedis(cfg.GetSection(RedisOptions.SectionName))
                .ConfigureRedis(opt => opt.Optional = true);
    }
}