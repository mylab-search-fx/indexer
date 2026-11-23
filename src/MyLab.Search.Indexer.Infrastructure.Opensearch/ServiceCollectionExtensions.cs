using Microsoft.Extensions.Configuration;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.DestinationRepository;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.Options;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.Serialization;
using MyLab.OpenSearch.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddOpenSearchInfrastructure()
        {
            srv.AddSingleton<IDestinationRepositoryFactory, OpenSearchDestinationRepositoryFactory>()
                .AddOpenSearch();

            srv.AddHealthChecks()
                .AddOpenSearch();

            return srv;
        }
    
        public IServiceCollection ConfigureOpenSearchInfrastructure(IConfiguration cfg)
        {
            return srv
                .AddSingleton<IDestinationRepositoryFactory, OpenSearchDestinationRepositoryFactory>()
                .AddOpenSearch()
                .ConfigureOpenSearch(cfg)
                .ConfigureOpenSearch(opt => opt.JsonSerializerOptions = IndexerJsonSerializerOptions.Options)
                .Configure<IndexingOptions>(cfg.GetSection(IndexingOptions.SectionName));
        }

        public IServiceCollection ConfigureOpenSearchInfrastructure
        (
            Action<IndexingOptions> configureIndexingOptions,
            Action<OpenSearchOptions> configureOpenSearchOptions
        )
        {
            return srv
                .AddSingleton<IDestinationRepositoryFactory, OpenSearchDestinationRepositoryFactory>()
                .AddOpenSearch()
                .ConfigureOpenSearch(configureOpenSearchOptions)
                .ConfigureOpenSearch(opt => opt.JsonSerializerOptions = IndexerJsonSerializerOptions.Options)
                .Configure(configureIndexingOptions);
        }
    }
}