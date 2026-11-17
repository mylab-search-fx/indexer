using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Redis;
using MyLab.Redis.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Options;
using MyLab.Search.Indexer.Infrastructure.FileSystem;
using MyLab.Search.Indexer.Infrastructure.Redis;

namespace MyLab.Search.Indexer.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndexerInfrastructure(this IServiceCollection srv, IConfiguration cfg)
        {
            return srv.AddSingleton<IIndexDescProvider, LocalFileIndexDescProvider>()
                .AddSingleton<IIndexLocker, RedisIndexLocker>()
                .AddRedis()
                .Configure<WorkerFileSystemOptions>(cfg.GetSection(WorkerOptions.SectionName))
                .ConfigureRedis(cfg.GetSection(RedisOptions.SectionName))
                .ConfigureRedis(opt => opt.Optional = true);
        }
    }
}
