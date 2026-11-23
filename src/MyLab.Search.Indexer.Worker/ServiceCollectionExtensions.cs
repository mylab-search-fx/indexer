using MyLab.Log;
using MyLab.Search.Indexer.Worker.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddWorker()
            => srv.AddIndexerAppLogic()
                .AddRedisInfrastructure()
                .AddFileSystemInfrastructure()
                .AddDatabaseInfrastructure()
                .AddOpenSearchInfrastructure()
                .AddLogging(c => c.AddMyLabConsole())
                .AddHostedService<IndexerBackgroundService>();

        public IServiceCollection ConfigureWorker(IConfiguration cfg)
            => srv.ConfigureIndexerAppLogic(cfg)
                .ConfigureDatabaseInfrastructure(cfg)
                .ConfigureFileSystemInfrastructure(cfg)
                .ConfigureOpenSearchInfrastructure(cfg)
                .ConfigureRedisInfrastructure(cfg);
    }
}