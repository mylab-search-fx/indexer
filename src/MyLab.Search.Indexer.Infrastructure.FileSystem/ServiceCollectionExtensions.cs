using Microsoft.Extensions.Configuration;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Options;
using MyLab.Search.Indexer.Infrastructure.FileSystem;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileSystemInfrastructure(this IServiceCollection srv, IConfiguration cfg)
        {
            return srv
                .AddSingleton<IIndexDescProvider, LocalFileIndexDescProvider>()
                .Configure<WorkerFileSystemOptions>(cfg.GetSection(WorkerOptions.SectionName));
        }
    }
}
