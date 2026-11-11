using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .Configure<WorkerFileSystemOptions>(cfg.GetSection(WorkerOptions.SectionName))
                .Configure<WorkerRedisOptions>(cfg.GetSection(WorkerRedisOptions.SectionName));
        }
    }
}
