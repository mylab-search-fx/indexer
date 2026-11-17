using Microsoft.Extensions.Configuration;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Infrastructure.Database;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;
using MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection srv, IConfiguration cfg)
    {
        return srv
            .AddSingleton<IIndexVersionController, DbIndexVersionController>()
            .AddSingleton<ISeedStorage, DbSeedStorage>()
            .AddSingleton<ISourceRepositoryFactory, DbSourceRepositoryFactory>()
            .AddDbTools(new SingleDbProviderSource(new MariaDb10DataProvider()))
            .ConfigureDbTools(cfg);
        
    }
}