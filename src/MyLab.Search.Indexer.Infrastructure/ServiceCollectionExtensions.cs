using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Infrastructure.FileSystem;

namespace MyLab.Search.Indexer.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndexerInfrastructure(this IServiceCollection srv)
        {
            return srv.AddSingleton<IIndexDescProvider, LocalFileIndexDescProvider>();
        }
    }
}
