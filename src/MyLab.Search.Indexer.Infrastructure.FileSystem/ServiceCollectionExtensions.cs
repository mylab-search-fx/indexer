using Microsoft.Extensions.Configuration;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Options;
using MyLab.Search.Indexer.Infrastructure.FileSystem;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddFileSystemInfrastructure()
            => srv.AddSingleton<IIndexDescProvider, LocalFileIndexDescProvider>();

        public IServiceCollection ConfigureFileSystemInfrastructure(IConfiguration cfg)
            => srv.Configure<WorkerFileSystemOptions>(cfg.GetSection(WorkerOptions.SectionName));

        public IServiceCollection ConfigureFileSystemInfrastructure(Action<WorkerFileSystemOptions> configure)
            => srv.Configure(configure);
    }
}